using UnityEditor;
using UnityEngine;

namespace CreativeVeinStudio.Simple_Pool_Manager.Helpers
{
    public static class CustomEditorStyles
    {
        public static RectOffset RectOffset(int left, int right, int top, int bottom) =>
            new RectOffset(left, right, top, bottom);

        public static RectOffset RectOffset(int val) => new RectOffset(val, val, val, val);

        public static Texture2D MakeTexture(int width, int height, Color col)
        {
            var pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

        public static GUIStyle ToggleStyle() => new GUIStyle(EditorStyles.miniButtonMid)
        {
            normal =
            {
                background = EditorGUIUtility.Load("node0") as Texture2D,
            },
            fixedHeight = 25,
            stretchWidth = true,
            richText = true,
            alignment = TextAnchor.MiddleCenter
        };

        public static GUIStyle CustomBoxStyle(Vector4 margin, Vector4 padding) => new GUIStyle()
        {
            margin = RectOffset((int) margin.x, (int) margin.y, (int) margin.z, (int) margin.w),
            padding = RectOffset((int) padding.x, (int) padding.y, (int) padding.z, (int) padding.w),
            stretchWidth = true,
            wordWrap = false,
        };

        public static GUIStyle CustomBoxStyle(Vector4 margin, Vector4 padding, Texture2D background) => new GUIStyle()
        {
            margin = RectOffset((int) margin.x, (int) margin.y, (int) margin.z, (int) margin.w),
            padding = RectOffset((int) padding.x, (int) padding.y, (int) padding.z, (int) padding.w),
            stretchWidth = true,
            wordWrap = false,
            normal = {background = background}
        };

        public static GUIStyle
            CustomLabel(TextAnchor alignment, int fontsize, Color color, Vector4 margin, Vector4 padding) =>
            new GUIStyle()
            {
                alignment = alignment,
                fontSize = fontsize,
                fontStyle = FontStyle.Bold,
                normal = {textColor = color},
                margin = RectOffset((int) margin.x, (int) margin.y, (int) margin.z, (int) margin.w),
                padding = RectOffset((int) padding.x, (int) padding.y, (int) padding.z, (int) padding.w),
            };

        public static GUIStyle CustomAreaStyle(Vector4 margin, Vector4 padding, Texture2D background) => new GUIStyle()
        {
            margin = RectOffset((int) margin.x, (int) margin.y, (int) margin.z, (int) margin.w),
            padding = RectOffset((int) padding.x, (int) padding.y, (int) padding.z, (int) padding.w),
            normal = {background = background}
        };
    }
}