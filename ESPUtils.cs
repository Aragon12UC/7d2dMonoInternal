using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExampleAssembly {
    static class ESPUtils {
        static ESPUtils() {
            whiteTexture = Texture2D.whiteTexture;
            drawingTex = new Texture2D(1, 1);

            drawMaterial = new Material(Shader.Find("Hidden/Internal-Colored"))
            {
                hideFlags = (HideFlags)61
            };

            drawMaterial.SetInt("_SrcBlend", 5);
            drawMaterial.SetInt("_DstBlend", 10);
            drawMaterial.SetInt("_Cull", 0);
            drawMaterial.SetInt("_ZWrite", 0);
        }

        public static Color GetHealthColour(float health, float maxHealth) {
            Color result = Color.green;

            float percentage = health / maxHealth;

            if (percentage >= 0.75f) {
                result = Color.green;
            } else {
                result = Color.yellow;
            }

            if (percentage <= 0.25f) {
                result = Color.red;
            }

            return result;
        }

        public static void DrawCircle(Color Col, Vector2 Center, float Radius) {
            GL.PushMatrix();

            if (!drawMaterial.SetPass(0)) {
                GL.PopMatrix();
                return;
            }

            GL.Begin(1);
            GL.Color(Col);

            for (float num = 0f; num < 6.28318548f; num += 0.05f) {
                GL.Vertex(new Vector3(Mathf.Cos(num) * Radius + Center.x, Mathf.Sin(num) * Radius + Center.y));
                GL.Vertex(new Vector3(Mathf.Cos(num + 0.05f) * Radius + Center.x, Mathf.Sin(num + 0.05f) * Radius + Center.y));
            }

            GL.End();
            GL.PopMatrix();
        }

        public static void DrawLine(Vector2 start, Vector2 end, Color color, float width) {
            Color oldColour = GUI.color;

            var rad2deg = 360 / (Math.PI * 2);

            Vector2 d = end - start;

            float a = (float)rad2deg * Mathf.Atan(d.y / d.x);

            if (d.x < 0)
                a += 180;

            int width2 = (int)Mathf.Ceil(width / 2);

            GUIUtility.RotateAroundPivot(a, start);

            GUI.color = color;

            GUI.DrawTexture(new Rect(start.x, start.y - width2, d.magnitude, width), Texture2D.whiteTexture, ScaleMode.StretchToFill);

            GUIUtility.RotateAroundPivot(-a, start);

            GUI.color = oldColour;
        }

        public static void OutlineBox(Vector2 pos, Vector2 size, Color colour) {
            Color oldColour = GUI.color;
            GUI.color = colour;

            GUI.DrawTexture(new Rect(pos.x, pos.y, 1, size.y), whiteTexture);
            GUI.DrawTexture(new Rect(pos.x + size.x, pos.y, 1, size.y), whiteTexture);
            GUI.DrawTexture(new Rect(pos.x, pos.y, size.x, 1), whiteTexture);
            GUI.DrawTexture(new Rect(pos.x, pos.y + size.y, size.x, 1), whiteTexture);

            GUI.color = oldColour;
        }

        public static bool IsOnScreen(Vector3 position) {
            return position.y > 0.01f && position.y < Screen.height - 5f && position.z > 0.01f;
        }

        public static void CornerBox(Vector2 Head, float Width, float Height, float thickness, Color color, bool outline) {
            int num = (int)(Width / 4f);
            int num2 = num;

            if (outline) {
                RectFilled(Head.x - Width / 2f - 1f, Head.y - 1f, num + 2, 3f, Color.black);
                RectFilled(Head.x - Width / 2f - 1f, Head.y - 1f, 3f, num2 + 2, Color.black);
                RectFilled(Head.x + Width / 2f - num - 1f, Head.y - 1f, num + 2, 3f, Color.black);
                RectFilled(Head.x + Width / 2f - 1f, Head.y - 1f, 3f, num2 + 2, Color.black);
                RectFilled(Head.x - Width / 2f - 1f, Head.y + Height - 4f, num + 2, 3f, Color.black);
                RectFilled(Head.x - Width / 2f - 1f, Head.y + Height - num2 - 4f, 3f, num2 + 2, Color.black);
                RectFilled(Head.x + Width / 2f - num - 1f, Head.y + Height - 4f, num + 2, 3f, Color.black);
                RectFilled(Head.x + Width / 2f - 1f, Head.y + Height - num2 - 4f, 3f, num2 + 3, Color.black);
            }

            RectFilled(Head.x - Width / 2f, Head.y, num, 1f, color);
            RectFilled(Head.x - Width / 2f, Head.y, 1f, num2, color);
            RectFilled(Head.x + Width / 2f - num, Head.y, num, 1f, color);
            RectFilled(Head.x + Width / 2f, Head.y, 1f, num2, color);
            RectFilled(Head.x - Width / 2f, Head.y + Height - 3f, num, 1f, color);
            RectFilled(Head.x - Width / 2f, Head.y + Height - num2 - 3f, 1f, num2, color);
            RectFilled(Head.x + Width / 2f - num, Head.y + Height - 3f, num, 1f, color);
            RectFilled(Head.x + Width / 2f, Head.y + Height - num2 - 3f, 1f, num2 + 1, color);
        }

        public static void RectFilled(float x, float y, float width, float height, Color color) {
            if (color != lastTexColour) {
                drawingTex.SetPixel(0, 0, color);
                drawingTex.Apply();

                lastTexColour = color;
            }

            GUI.DrawTexture(new Rect(x, y, width, height), drawingTex);
        }

        public static void DrawString(Vector2 pos, string text, Color color, bool center = true, int size = 12, FontStyle fontStyle = FontStyle.Bold, int depth = 1) {
            __style.fontSize = size;
            __style.richText = true;
            __style.normal.textColor = color;
            __style.fontStyle = fontStyle;

            __outlineStyle.fontSize = size;
            __outlineStyle.richText = true;
            __outlineStyle.normal.textColor = new Color(0f, 0f, 0f, 1f);
            __outlineStyle.fontStyle = fontStyle;

            GUIContent content = new GUIContent(text);
            GUIContent content2 = new GUIContent(text);
            if (center) {
                //GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                pos.x -= __style.CalcSize(content).x / 2f;
            }
            switch (depth) {
                case 0:
                    GUI.Label(new Rect(pos.x, pos.y, 300f, 25f), content, __style);
                    break;
                case 1:
                    GUI.Label(new Rect(pos.x + 1f, pos.y + 1f, 300f, 25f), content2, __outlineStyle);
                    GUI.Label(new Rect(pos.x, pos.y, 300f, 25f), content, __style);
                    break;
                case 2:
                    GUI.Label(new Rect(pos.x + 1f, pos.y + 1f, 300f, 25f), content2, __outlineStyle);
                    GUI.Label(new Rect(pos.x - 1f, pos.y - 1f, 300f, 25f), content2, __outlineStyle);
                    GUI.Label(new Rect(pos.x, pos.y, 300f, 25f), content, __style);
                    break;
                case 3:
                    GUI.Label(new Rect(pos.x + 1f, pos.y + 1f, 300f, 25f), content2, __outlineStyle);
                    GUI.Label(new Rect(pos.x - 1f, pos.y - 1f, 300f, 25f), content2, __outlineStyle);
                    GUI.Label(new Rect(pos.x, pos.y - 1f, 300f, 25f), content2, __outlineStyle);
                    GUI.Label(new Rect(pos.x, pos.y + 1f, 300f, 25f), content2, __outlineStyle);
                    GUI.Label(new Rect(pos.x, pos.y, 300f, 25f), content, __style);
                    break;
            }
        }

        private static Texture2D drawingTex;
        private static Texture2D whiteTexture;

        private static Color lastTexColour;

        private static Material drawMaterial;

        private static GUIStyle __style = new GUIStyle();
        private static GUIStyle __outlineStyle = new GUIStyle();
    }
}
