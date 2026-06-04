using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public static class GameplayChineseText
{
    static Font cachedUiFont;
    static readonly HashSet<char> PreparedCharacters = new HashSet<char>();
    static readonly StringBuilder SharedBuilder = new StringBuilder(256);

    static readonly string CommonChineseCharacters =
        "随身可建造工作台熔炉锻造台织布台炼金台科技研究台木箱大型储物箱木墙石墙木地板石地板" +
        "木材石头草纤维藤蔓燧石黏土矿石碎片煤炭浆果木板木棍绳子布料砖块金属锭" +
        "石斧石镐简易背包营火绷带基础剑钥匙碎片红色橙色黄色绿色青色蓝色紫色粉色白灰" +
        "制作关闭背包材料不足背包已满已收回已制作需要解锁科技需要先建造放置位置无效无法在此放置左键放置准星对准地面使用" +
        "目标制造并放置工作台使用熔炉前往已激活的信标进入下一关信标已激活和下一关入口已开启图鉴制作图鉴滚轮浏览只读参考实际制造请用制造面板分类状态可制作暂不可制作" +
        "手持工具教学石镐采矿石斧伐木修理工具修复信号中继器矿点木桩左键或F使用距离太远无法使用工具" +
        "工具教学完成三项任务已完成前往传送门并按Y进入第七关第七关传送门按Y进入下一关" +
        "选择石镐并采矿一次选择石斧并伐木一次选择修理工具并修复信号中继器采矿区伐木区修复区";

    public static Font GetUiFont()
    {
        if (cachedUiFont != null)
        {
            return cachedUiFont;
        }

        cachedUiFont = Font.CreateDynamicFontFromOSFont(
            new[] { "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "PingFang SC", "Arial Unicode MS", "Arial" },
            32);

        if (cachedUiFont == null)
        {
            cachedUiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        PrepareFontForString(cachedUiFont, CommonChineseCharacters, 32, FontStyle.Normal);
        return cachedUiFont;
    }

    public static void PrepareUiText(Text textComponent, string content)
    {
        if (textComponent == null)
        {
            return;
        }

        Font font = GetUiFont();
        textComponent.font = font;
        if (!string.IsNullOrEmpty(content))
        {
            PrepareFontForString(font, content, textComponent.fontSize, textComponent.fontStyle);
        }

        textComponent.text = content ?? string.Empty;
    }

    public static void ApplyWorldLabel(Text textComponent, string content, int fontSize = 40)
    {
        if (textComponent == null)
        {
            return;
        }

        Font font = GetUiFont();
        string display = content ?? string.Empty;
        PrepareFontForString(font, display, fontSize, FontStyle.Bold);
        PrepareFontForString(font, CommonChineseCharacters, fontSize, FontStyle.Bold);

        textComponent.font = font;
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = FontStyle.Bold;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
        textComponent.verticalOverflow = VerticalWrapMode.Overflow;
        textComponent.color = Color.white;
        textComponent.text = display;
    }

    public static void ApplyWorldLabel(TextMesh textMesh, string content, int fontSize = 48, float characterSize = 0.07f)
    {
        if (textMesh == null)
        {
            return;
        }

        Font font = GetUiFont();
        string display = content ?? string.Empty;
        PrepareFontForString(font, display, fontSize, FontStyle.Bold);
        PrepareFontForString(font, CommonChineseCharacters, fontSize, FontStyle.Bold);

        textMesh.font = font;
        textMesh.fontSize = fontSize;
        textMesh.characterSize = characterSize;
        textMesh.fontStyle = FontStyle.Bold;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;
        textMesh.text = display;
    }

    static void PrepareFontForString(Font font, string text, int size, FontStyle style)
    {
        if (font == null || string.IsNullOrEmpty(text))
        {
            return;
        }

        SharedBuilder.Clear();
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (PreparedCharacters.Add(c))
            {
                SharedBuilder.Append(c);
            }
        }

        if (SharedBuilder.Length > 0)
        {
            font.RequestCharactersInTexture(SharedBuilder.ToString(), size, style);
        }
    }
}
