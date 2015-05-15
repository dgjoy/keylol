using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Keylol.FontGarage.Table.Glyf
{
    [Flags]
    public enum ComponentFlags : ushort
    {
        None = 0,
        Arg1And2AreWords = 1,
        ArgsAreXyValues = 1 << 1,
        RoundXyToGrid = 1 << 2,
        WeHaveAScale = 1 << 3,
        MoreComponents = 1 << 5,
        WeHaveAnXAndYScale = 1 << 6,
        WeHaveATwoByTwo = 1 << 7,
        WeHaveInstructions = 1 << 8,
        UseMyMetrics = 1 << 9,
        OverlapCompound = 1 << 10,
        ScaledComponentOffset = 1 << 11,
        UnscaledComponentOffset = 1 << 12
    }

    public class GlyphComponent : IDeepCopyable
    {
        public ComponentFlags Flags { get; set; }
        public ushort GlyphId { get; set; }
        public byte[] TransformationData { get; set; }

        public object DeepCopy()
        {
            var newComponent = (GlyphComponent) MemberwiseClone();
            newComponent.TransformationData = (byte[]) TransformationData.Clone();
            return newComponent;
        }
    }

    public class CompositeGlyph : Glyph
    {
        public CompositeGlyph()
        {
            Components = new List<GlyphComponent>();
            Instructions = new byte[0];
        }

        public short XMin { get; set; }
        public short YMin { get; set; }
        public short XMax { get; set; }
        public short YMax { get; set; }
        public List<GlyphComponent> Components { get; set; }
        public byte[] Instructions { get; set; }

        public override void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font)
        {
            writer.BaseStream.Position = startOffset;
            DataTypeConverter.WriteShort(writer, -1);
            DataTypeConverter.WriteShort(writer, XMin);
            DataTypeConverter.WriteShort(writer, YMin);
            DataTypeConverter.WriteShort(writer, XMax);
            DataTypeConverter.WriteShort(writer, YMax);
            foreach (var component in Components)
            {
                DataTypeConverter.WriteUShort(writer, (ushort) component.Flags);
                DataTypeConverter.WriteUShort(writer, component.GlyphId);
                writer.Write(component.TransformationData);
            }
            DataTypeConverter.WriteUShort(writer, (ushort) Instructions.Length);
            writer.Write(Instructions);
        }

        public static CompositeGlyph Deserialize(BinaryReader reader, long startOffset)
        {
            var glyph = new CompositeGlyph();
            reader.BaseStream.Position = startOffset + DataTypeLength.Short;

            glyph.XMin = DataTypeConverter.ReadShort(reader);
            glyph.YMin = DataTypeConverter.ReadShort(reader);
            glyph.XMax = DataTypeConverter.ReadShort(reader);
            glyph.YMax = DataTypeConverter.ReadShort(reader);

            do
            {
                var component = new GlyphComponent
                {
                    Flags = (ComponentFlags) DataTypeConverter.ReadUShort(reader),
                    GlyphId = DataTypeConverter.ReadUShort(reader)
                };
                var dataLength = 0;
                if (component.Flags.HasFlag(ComponentFlags.Arg1And2AreWords))
                    dataLength += DataTypeLength.UShort*2;
                else
                    dataLength += DataTypeLength.UShort;
                if (component.Flags.HasFlag(ComponentFlags.WeHaveAScale))
                    dataLength += DataTypeLength.F2Dot14;
                else if (component.Flags.HasFlag(ComponentFlags.WeHaveAnXAndYScale))
                    dataLength += DataTypeLength.F2Dot14*2;
                else if (component.Flags.HasFlag(ComponentFlags.WeHaveATwoByTwo))
                    dataLength += DataTypeLength.F2Dot14*4;
                component.TransformationData = reader.ReadBytes(dataLength);
                glyph.Components.Add(component);
            } while (glyph.Components.Last().Flags.HasFlag(ComponentFlags.MoreComponents));
            if (glyph.Components.Last().Flags.HasFlag(ComponentFlags.WeHaveInstructions))
                glyph.Instructions = reader.ReadBytes(DataTypeConverter.ReadUShort(reader));
            return glyph;
        }

        public override object DeepCopy()
        {
            var newGlyph = (CompositeGlyph) MemberwiseClone();
            newGlyph.Components = Components.Select(component => (GlyphComponent) component.DeepCopy()).ToList();
            newGlyph.Instructions = (byte[]) Instructions.Clone();
            return newGlyph;
        }
    }
}