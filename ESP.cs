using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using O = ExampleAssembly.Objects;
using System.Runtime.InteropServices;

namespace ExampleAssembly {
    public class ESP : MonoBehaviour {
        private void Start() {
            // Camera.main is a very expensive getter, so we want to do it once and cache the result.
            mainCam = Camera.main;

            blackCol = new Color(0f, 0f, 0f, 120f);
            entityBoxCol = new Color(0.42f, 0.36f, 0.90f, 1f);
            crosshairCol = new Color32(30, 144, 255, 255);
        }

        private void Update() {
            if (zombieCornerBox) {
                zombieBox = false;
            } else if (zombieBox && zombieCornerBox) {
                zombieCornerBox = false;
            }

            if (playerCornerBox) {
                playerBox = false;
            } else if (playerBox && playerCornerBox) {
                playerCornerBox = false;
            }

            if (O.localPlayer) {
                O.localPlayer.weaponCrossHairAlpha = crosshair ? 0f : 255f;
            }
        }

        private void OnGUI() {
            // Run once per frame.
            if (Event.current.type != EventType.Repaint) {
                return;
            }

            if (!mainCam) {
                mainCam = Camera.main;
            }

            if (fovCircle) {
                // Outline
                ESPUtils.DrawCircle(Color.black, new Vector2(Screen.width / 2, Screen.height / 2), 149f);
                ESPUtils.DrawCircle(Color.black, new Vector2(Screen.width / 2, Screen.height / 2), 151f);

                ESPUtils.DrawCircle(new Color32(30, 144, 255, 255), new Vector2(Screen.width / 2, Screen.height / 2), 150f);
            }
            
            if (crosshair) {
                // Constantly redefining these vectors so that you can change your resolution and the crosshair will still be in the middle.
                Vector2 lineHorizontalStart = new Vector2(Screen.width / 2 - crosshairScale, Screen.height / 2);
                Vector2 lineHorizontalEnd = new Vector2(Screen.width / 2 + crosshairScale, Screen.height / 2);

                Vector2 lineVerticalStart = new Vector2(Screen.width / 2, Screen.height / 2 - crosshairScale);
                Vector2 lineVerticalEnd = new Vector2(Screen.width / 2, Screen.height / 2 + crosshairScale);

                ESPUtils.DrawLine(lineHorizontalStart, lineHorizontalEnd, crosshairCol, lineThickness);
                ESPUtils.DrawLine(lineVerticalStart, lineVerticalEnd, crosshairCol, lineThickness);
            }

            if (O.zombieList.Count > 0 && (zombieName || zombieBox || zombieHealth)) {
                foreach (EntityZombie zombie in O.zombieList) {
                    if (!zombie || !zombie.IsAlive()) {
                        continue;
                    }

                    Vector3 w2s = mainCam.WorldToScreenPoint(zombie.transform.position);

                    if (ESPUtils.IsOnScreen(w2s)) {
                        Vector3 w2sHead = mainCam.WorldToScreenPoint(zombie.emodel.GetHeadTransform().position);
                        
                        float height = Mathf.Abs(w2sHead.y - w2s.y);
                        float x = w2s.x - height * 0.3f /* Shift the box to the left a bit. */;
                        float y = Screen.height - w2sHead.y;

                        if (zombieBox) {
                            ESPUtils.OutlineBox(new Vector2(x - 1f, y - 1f), new Vector2((height / 2f) + 2f, height + 2f), blackCol);
                            ESPUtils.OutlineBox(new Vector2(x, y), new Vector2(height / 2f, height), entityBoxCol);
                            ESPUtils.OutlineBox(new Vector2(x + 1f, y + 1f), new Vector2((height / 2f) - 2f, height - 2f), blackCol);
                        } else if (zombieCornerBox) {
                            ESPUtils.CornerBox(new Vector2(w2sHead.x, y ), height / 2f, height, 2f, entityBoxCol, true);
                        }

                        if (zombieName) {
                            ESPUtils.DrawString(new Vector2(w2s.x, Screen.height - w2s.y + 8f/* Extra spacing below the box esp. */),
                            zombie.EntityName.Replace("zombie", "Zombie_"), Color.red, true, 12, FontStyle.Normal);
                        }

                        if (zombieHealth) {
                            float health = zombie.Health;
                            int maxHealth = zombie.GetMaxHealth();
                            float percentage = health / maxHealth;
                            float barHeight = height * percentage;

                            Color barColour = ESPUtils.GetHealthColour(health, maxHealth);

                            ESPUtils.RectFilled(x - 5f, y, 4f, height, blackCol);
                            ESPUtils.RectFilled(x - 4f, y + height - barHeight - 1f, 2f, barHeight, barColour);
                        }
                    }
                }
            }

            if (O.PlayerList.Count > 1 && (playerName || playerBox || playerHealth)) {
                foreach (EntityPlayer player in O.PlayerList) {
                    if (!player || player == O.localPlayer || !player.IsAlive()) {
                        continue;
                    }

                    Vector3 w2s = mainCam.WorldToScreenPoint(player.transform.position);

                    if (ESPUtils.IsOnScreen(w2s)) {
                        Vector3 w2sHead = mainCam.WorldToScreenPoint(player.emodel.GetHeadTransform().position);

                        float height = Mathf.Abs(w2sHead.y - w2s.y);
                        float x = w2s.x - height * 0.3f ;
                        float y = Screen.height - w2sHead.y;

                        if (zombieBox) {
                            ESPUtils.OutlineBox(new Vector2(x - 1f, y - 1f), new Vector2((height / 2f) + 2f, height + 2f), blackCol);
                            ESPUtils.OutlineBox(new Vector2(x, y), new Vector2(height / 2f, height), entityBoxCol);
                            ESPUtils.OutlineBox(new Vector2(x + 1f, y + 1f), new Vector2((height / 2f) - 2f, height - 2f), blackCol);
                        }
                        else if (zombieCornerBox) {
                            ESPUtils.CornerBox(new Vector2(w2sHead.x, y), height / 2f, height, 2f, entityBoxCol, true);
                        }

                        if (playerName) {
                            ESPUtils.DrawString(new Vector2(w2s.x, Screen.height - w2s.y + 8f),
                            player.EntityName, Color.red, true, 12, FontStyle.Normal);
                        }

                        if (playerHealth) {
                            float health = player.Health;
                            int maxHealth = player.GetMaxHealth();
                            float percentage = health / maxHealth;
                            float barHeight = height * percentage;

                            Color barColour = ESPUtils.GetHealthColour(health, maxHealth);

                            ESPUtils.RectFilled(x - 5f, y, 4f, height, blackCol);
                            ESPUtils.RectFilled(x - 4f, y + height - barHeight - 1f, 2f, barHeight, barColour);
                        }
                    }
                }
            }
        }

        public static Camera mainCam;

        private Color blackCol;
        private Color entityBoxCol;
        private Color crosshairCol;

        private readonly float crosshairScale = 14f;
        private readonly float lineThickness = 1.75f;

        public static bool playerBox = true, playerName = true, playerHealth = true, playerCornerBox = false;
        public static bool zombieBox = true, zombieName = true, zombieHealth = true, zombieCornerBox = false;
        public static bool crosshair = true, fovCircle  = true;
    }
}
