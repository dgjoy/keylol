using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class GlyphComponent
    {
        public ComponentFlags Flags { get; set; }
        public ushort GlyphId { get; set; }
        public byte[] TransformationData { get; set; }
    }

    public class CompositeGlyph : Glyph
    {
        public short XMin { get; set; }
        public short YMin { get; set; }
        public short XMax { get; set; }
        public short YMax { get; set; }

        public ComponentFlags Flags
        {
            get { return Components.First().Flags; }
        }

        public List<GlyphComponent> Components { get; set; }
        public byte[] Instructions { get; set; }

        public override void Serialize(BinaryWriter writer)
        {
            throw new NotImplementedException();
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
                var component = new GlyphComponent();
                component.Flags = (ComponentFlags) DataTypeConverter.ReadUShort(reader);
                component.GlyphId = DataTypeConverter.ReadUShort(reader);
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
            if (glyph.Flags.HasFlag(ComponentFlags.WeHaveInstructions))
            {
                glyph.Instructions = reader.ReadBytes(DataTypeConverter.ReadUShort(reader));
            }
            return glyph;
        }

        public CompositeGlyph()
        {
            Components = new List<GlyphComponent>();
        }
    }
}