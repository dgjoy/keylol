using System;
using System.Text;
using System.IO;
using CsQuery;
using CsQuery.StringScanner;
using CsQuery.Output;

namespace Keylol.Utilities
{
    /// <summary>
    /// A formatter that converts a DOM to a basic plain-text version.
    /// </summary>
    public class PlainTextFormatter : IOutputFormatter
    {
        private IStringInfo _stringInfo;
        private readonly bool _keepNewLine;

        private static readonly PlainTextFormatter DefaultKeepNewLine = new PlainTextFormatter(true);
        private static readonly PlainTextFormatter DefaultRemoveNewLine = new PlainTextFormatter(false);

        private PlainTextFormatter(bool keepNewLine)
        {
            _keepNewLine = keepNewLine;
        }

        /// <summary>
        /// 扁平化 HTML 富文本
        /// </summary>
        /// <param name="html">HTML 代码</param>
        /// <param name="keepNewLine">是否保留换行</param>
        /// <returns>扁平化后的文本</returns>
        public static string FlattenHtml(string html, bool keepNewLine)
        {
            return CQ.Create($"<div>{html}</div>").Render(keepNewLine ? DefaultKeepNewLine : DefaultRemoveNewLine);
        }

        /// <summary>
        /// Renders this object to the passed TextWriter.
        /// </summary>
        ///
        /// <param name="node">
        /// The node.
        /// </param>
        /// <param name="writer">
        /// The writer.
        /// </param>
        public void Render(IDomObject node, TextWriter writer)
        {
            _stringInfo = CharacterData.CreateStringInfo();

            var sb = new StringBuilder();
            AddContents(sb, node, true);
            writer.Write(sb.ToString().TrimEnd());
        }

        /// <summary>
        /// Renders this object and returns the output as a string.
        /// </summary>
        ///
        /// <param name="node">
        /// The node.
        /// </param>
        ///
        /// <returns>
        /// A string of HTML.
        /// </returns>
        public string Render(IDomObject node)
        {
            using (var writer = new StringWriter())
            {
                Render(node, writer);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Adds the contents to 'node' to the StringBuilder.
        /// </summary>
        ///
        /// <param name="sb">
        /// The StringBuilder.
        /// </param>
        /// <param name="node">
        /// The node.
        /// </param>
        /// <param name="skipWhitespace">
        /// true to skip any leading whitespace for this node.
        /// </param>
        protected void AddContents(StringBuilder sb, IDomObject node, bool skipWhitespace)
        {
            // always skip the opening whitespace of a new child block
            if (!node.HasChildren) return;
            foreach (var el in node.ChildNodes)
            {
                if (el.NodeType == NodeType.TEXT_NODE)
                {
                    _stringInfo.Target = el.NodeValue;

                    if (_stringInfo.Whitespace)
                    {
                        if (skipWhitespace) continue;
                        sb.Append(" ");
                        skipWhitespace = true;
                    }
                    else
                    {
                        var val = CleanFragment(el.Render());
                        if (skipWhitespace)
                        {
                            val = val.TrimStart();
                            skipWhitespace = false;
                        }

                        sb.Append(val);
                    }
                }
                else if (el.NodeType == NodeType.ELEMENT_NODE)
                {
                    var elNode = (IDomElement) el;
                    // first add any inner contents

                    if (el.NodeName == "HEAD" || el.NodeName == "STYLE" || el.NodeName == "SCRIPT") continue;
                    if (_keepNewLine)
                    {
                        if (elNode.NodeName == "BR")
                        {
                            sb.Append(Environment.NewLine);
                        }
                        else if (elNode.NodeName == "PRE")
                        {
                            RemoveTrailingWhitespace(sb);
                            sb.Append(Environment.NewLine);
                            sb.Append(ToStandardLineEndings(el.InnerText));
                            RemoveTrailingWhitespace(sb);
                            sb.Append(Environment.NewLine);
                            skipWhitespace = true;
                        }
                    }
                    if (elNode.NodeName == "IMG")
                    {
                        sb.Append("〔附图〕");
                    }
                    else
                    {
                        if (elNode.IsBlock && sb.Length > 0)
                        {
                            RemoveTrailingWhitespace(sb);
                            sb.Append(_keepNewLine ? Environment.NewLine : " ");
                        }

                        AddContents(sb, el, elNode.IsBlock);
                        RemoveTrailingWhitespace(sb);

                        if (!elNode.IsBlock) continue;
                        sb.Append(_keepNewLine ? Environment.NewLine : " ");
                        skipWhitespace = true;
                    }
                }
            }
        }

        /// <summary>
        /// Converts the newline characters in a string to standard system line endings
        /// </summary>
        ///
        /// <param name="text">
        /// The text.
        /// </param>
        ///
        /// <returns>
        /// The converted string
        /// </returns>
        protected string ToStandardLineEndings(string text)
        {
            return text.Replace("\r\n", "\n").Replace("\n", "\r\n");
        }

        /// <summary>
        /// Removes trailing whitespace in this StringBuilder
        /// </summary>
        ///
        /// <param name="sb">
        /// The StringBuilder.
        /// </param>
        protected void RemoveTrailingWhitespace(StringBuilder sb)
        {
            // erase ending whitespace -- scan backwards until non-whitespace

            var i = sb.Length - 1;
            var count = 0;
            while (i >= 0 && CharacterData.IsType(sb[i], CharacterType.Whitespace))
            {
                i--;
                count++;
            }
            if (i < sb.Length - 1)
            {
                sb.Remove(i + 1, count);
            }
        }

        /// <summary>
        /// Clean a string fragment for output as text
        /// </summary>
        ///
        /// <param name="text">
        /// The text.
        /// </param>
        ///
        /// <returns>
        /// The clean text
        /// </returns>
        protected string CleanFragment(string text)
        {
            var charInfo = CharacterData.CreateCharacterInfo();

            var sb = new StringBuilder();
            var index = 0;
            var trimmed = true;
            while (index < text.Length)
            {
                charInfo.Target = text[index];
                if (!trimmed && !charInfo.Whitespace)
                {
                    trimmed = true;
                }
                if (trimmed)
                {
                    if (charInfo.Whitespace)
                    {
                        // convert all whitespace blocks into a single space
                        sb.Append(" ");
                        trimmed = false;
                    }
                    else
                    {
                        sb.Append(text[index]);
                    }
                }
                index++;
            }

            return sb.ToString();
        }
    }
}