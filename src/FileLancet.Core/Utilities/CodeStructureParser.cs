using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FileLancet.Core.Utilities;

/// <summary>
/// 代码结构节点
/// </summary>
public class CodeStructureNode
{
    /// <summary>
    /// 节点类型
    /// </summary>
    public CodeNodeType NodeType { get; set; }

    /// <summary>
    /// 节点名称（标签名或属性名）
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 节点值（文本内容或属性值）
    /// </summary>
    public string Value { get; set; } = "";

    /// <summary>
    /// 完整内容
    /// </summary>
    public string Content { get; set; } = "";

    /// <summary>
    /// 子节点
    /// </summary>
    public List<CodeStructureNode> Children { get; set; } = new();

    /// <summary>
    /// 属性列表（仅用于元素节点）
    /// </summary>
    public Dictionary<string, string> Attributes { get; set; } = new();

    /// <summary>
    /// 行号
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// 缩进级别
    /// </summary>
    public int IndentLevel { get; set; }

    /// <summary>
    /// 显示文本
    /// </summary>
    public string DisplayText
    {
        get
        {
            return NodeType switch
            {
                CodeNodeType.Element => $"<{Name}>{GetAttributesText()}",
                CodeNodeType.Text => $"\"{Truncate(Value, 50)}\"",
                CodeNodeType.Attribute => $"{Name}=\"{Value}\"",
                CodeNodeType.Comment => $"<!-- {Truncate(Value, 30)} -->",
                CodeNodeType.CssRule => $"{Name} {{...}}",
                CodeNodeType.CssProperty => $"{Name}: {Value};",
                _ => Content
            };
        }
    }

    private string GetAttributesText()
    {
        if (Attributes.Count == 0) return "";
        var attrs = string.Join(" ", Attributes.Select(a => $"{a.Key}=\"{a.Value}\""));
        return $" ({attrs})";
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return "";
        value = value.Replace("\n", " ").Replace("\r", "").Trim();
        if (value.Length <= maxLength) return value;
        return value.Substring(0, maxLength) + "...";
    }
}

/// <summary>
/// 代码节点类型
/// </summary>
public enum CodeNodeType
{
    Root,
    Element,
    Text,
    Attribute,
    Comment,
    CssRule,
    CssProperty,
    CssSelector
}

/// <summary>
/// 代码结构解析器
/// </summary>
public class CodeStructureParser
{
    /// <summary>
    /// 解析 XML/HTML 内容
    /// </summary>
    public static CodeStructureNode ParseXml(string content)
    {
        var root = new CodeStructureNode
        {
            NodeType = CodeNodeType.Root,
            Name = "Document",
            Content = content
        };

        try
        {
            // 尝试作为 XML 解析
            var doc = XDocument.Parse(content);
            if (doc.Root != null)
            {
                root.Children.Add(ParseXElement(doc.Root, 0));
            }
        }
        catch
        {
            // XML 解析失败，使用简单的标签提取
            root.Children.AddRange(ParseHtmlLoosely(content));
        }

        return root;
    }

    /// <summary>
    /// 解析 CSS 内容
    /// </summary>
    public static CodeStructureNode ParseCss(string content)
    {
        var root = new CodeStructureNode
        {
            NodeType = CodeNodeType.Root,
            Name = "CSS",
            Content = content
        };

        // 简单的 CSS 规则解析
        var rulePattern = @"([^{]+)\{([^}]*)\}";
        var matches = Regex.Matches(content, rulePattern, RegexOptions.Singleline);

        int lineNum = 1;
        foreach (Match match in matches)
        {
            var selector = match.Groups[1].Value.Trim();
            var properties = match.Groups[2].Value.Trim();

            var ruleNode = new CodeStructureNode
            {
                NodeType = CodeNodeType.CssRule,
                Name = selector,
                Content = match.Value,
                LineNumber = lineNum
            };

            // 解析属性
            var propLines = properties.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var propLine in propLines)
            {
                var parts = propLine.Split(':', 2);
                if (parts.Length == 2)
                {
                    ruleNode.Children.Add(new CodeStructureNode
                    {
                        NodeType = CodeNodeType.CssProperty,
                        Name = parts[0].Trim(),
                        Value = parts[1].Trim(),
                        LineNumber = lineNum
                    });
                }
            }

            root.Children.Add(ruleNode);
            lineNum++;
        }

        return root;
    }

    /// <summary>
    /// 解析纯文本内容（按行）
    /// </summary>
    public static CodeStructureNode ParseText(string content)
    {
        var root = new CodeStructureNode
        {
            NodeType = CodeNodeType.Root,
            Name = "Text",
            Content = content
        };

        var lines = content.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            root.Children.Add(new CodeStructureNode
            {
                NodeType = CodeNodeType.Text,
                Name = $"Line {i + 1}",
                Value = lines[i].TrimEnd(),
                LineNumber = i + 1
            });
        }

        return root;
    }

    private static CodeStructureNode ParseXElement(XElement element, int indentLevel)
    {
        var node = new CodeStructureNode
        {
            NodeType = CodeNodeType.Element,
            Name = element.Name.LocalName,
            IndentLevel = indentLevel,
            Content = element.ToString()
        };

        // 添加属性
        foreach (var attr in element.Attributes())
        {
            node.Attributes[attr.Name.LocalName] = attr.Value;
        }

        // 处理子节点
        foreach (var child in element.Nodes())
        {
            switch (child)
            {
                case XElement childElement:
                    node.Children.Add(ParseXElement(childElement, indentLevel + 1));
                    break;
                case XText text:
                    if (!string.IsNullOrWhiteSpace(text.Value))
                    {
                        node.Children.Add(new CodeStructureNode
                        {
                            NodeType = CodeNodeType.Text,
                            Value = text.Value.Trim(),
                            IndentLevel = indentLevel + 1
                        });
                    }
                    break;
                case XComment comment:
                    node.Children.Add(new CodeStructureNode
                    {
                        NodeType = CodeNodeType.Comment,
                        Value = comment.Value.Trim(),
                        IndentLevel = indentLevel + 1
                    });
                    break;
            }
        }

        return node;
    }

    private static List<CodeStructureNode> ParseHtmlLoosely(string content)
    {
        var nodes = new List<CodeStructureNode>();
        var tagPattern = @"<(/?)([a-zA-Z][a-zA-Z0-9]*)[^>]*?(/?)>";
        var matches = Regex.Matches(content, tagPattern);

        int lastIndex = 0;
        foreach (Match match in matches)
        {
            // 添加标签前的文本
            if (match.Index > lastIndex)
            {
                var text = content.Substring(lastIndex, match.Index - lastIndex).Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    nodes.Add(new CodeStructureNode
                    {
                        NodeType = CodeNodeType.Text,
                        Value = text
                    });
                }
            }

            var tagName = match.Groups[2].Value;
            var isClosing = match.Groups[1].Value == "/";
            var isSelfClosing = match.Groups[3].Value == "/";

            nodes.Add(new CodeStructureNode
            {
                NodeType = CodeNodeType.Element,
                Name = tagName,
                Content = match.Value,
                Attributes = ExtractAttributes(match.Value)
            });

            lastIndex = match.Index + match.Length;
        }

        // 添加剩余文本
        if (lastIndex < content.Length)
        {
            var text = content.Substring(lastIndex).Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                nodes.Add(new CodeStructureNode
                {
                    NodeType = CodeNodeType.Text,
                    Value = text
                });
            }
        }

        return nodes;
    }

    private static Dictionary<string, string> ExtractAttributes(string tag)
    {
        var attrs = new Dictionary<string, string>();
        var attrPattern = "(\\w+)=[\"']([^\"']*)[\"']";
        var matches = Regex.Matches(tag, attrPattern);

        foreach (Match match in matches)
        {
            attrs[match.Groups[1].Value] = match.Groups[2].Value;
        }

        return attrs;
    }
}
