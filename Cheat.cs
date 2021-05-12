using System;
using System.Runtime.InteropServices;
using UnityEngine;
using O = ExampleAssembly.Objects;

namespace ExampleAssembly
{
    public class Cheat : MonoBehaviour {
        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        private void Start() {
            lastChamTime = Time.time + 10f;

            chamsMaterial = new Material(Shader.Find("Hidden/Internal-Colored")) {
                hideFlags = HideFlags.HideAndDontSave
            };
            
            /* 
             * Unity hases the ID of every property name you feed it, 
             * so we're hashing it once instead of every time we want to use it.
             */
            _Color = Shader.PropertyToID("_Color");

            chamsMaterial.SetInt("_SrcBlend", 5);
            chamsMaterial.SetInt("_DstBlend", 10);
            chamsMaterial.SetInt("_Cull", 0);
            chamsMaterial.SetInt("_ZTest", 8); // 8 = see through walls.
            chamsMaterial.SetInt("_ZWrite", 0);
            chamsMaterial.SetColor(_Color, Color.magenta);
        }

        private void Aimbot() {
            // This aimbot is pasted, if you wrote this let me know and I'll credit you.
            float minDist = 9999f;

            Vector2 target = Vector2.zero;

            foreach (EntityZombie zombie in O.zombieList)
                if (zombie && zombie.IsAlive()) {
                    Vector3 lookAt = zombie.emodel.GetBellyPosition();
                    Vector3 w2s = ESP.mainCam.WorldToScreenPoint(lookAt);

                    // If they're outside of our FOV.
                    if (Vector2.Distance(new Vector2(Screen.width / 2, Screen.height / 2), new Vector2(w2s.x, w2s.y)) > 150f)
                        continue;

                    if (ESPUtils.IsOnScreen(w2s)) {
                        float distance = Math.Abs(Vector2.Distance(new Vector2(w2s.x, Screen.height - w2s.y), new Vector2(Screen.width / 2, Screen.height / 2)));

                        if (distance < minDist) {
                            minDist = distance;
                            target = new Vector2(w2s.x, Screen.height - w2s.y);
                        }
                    }
                }

            if (O.PlayerList.Count > 1) {
                foreach (EntityPlayer player in O.PlayerList) {
                    if (player && player.IsAlive() && player != O.localPlayer) {
                        Vector3 lookAt = player.emodel.GetBellyPosition();
                        Vector3 w2s = ESP.mainCam.WorldToScreenPoint(lookAt);

                        if (Vector2.Distance(new Vector2(Screen.width / 2, Screen.height / 2), new Vector2(w2s.x, w2s.y)) > 150f)
                            continue;

                        if (ESPUtils.IsOnScreen(w2s)) {
                            float distance = Math.Abs(Vector2.Distance(new Vector2(w2s.x, Screen.height - w2s.y), new Vector2(Screen.width / 2, Screen.height / 2)));

                            if (distance < minDist) {
                                minDist = distance;
                                target = new Vector2(w2s.x, Screen.height - w2s.y);
                            }
                        }
                    }
                }
            }

            if (target != Vector2.zero) {
                double distX = target.x - Screen.width / 2f;
                double distY = target.y - Screen.height / 2f;

                distX /= 10;
                distY /= 10;

                mouse_event(0x0001, (int)distX, (int)distY, 0, 0);
            }
        }

        private void MagicBullet() {
            EntityZombie ztarget = null;
            EntityPlayer pTarget = null;

            foreach (EntityZombie zombie in O.zombieList)
                if (zombie && zombie.IsAlive()) {
                    Vector3 head = zombie.emodel.GetHeadTransform().position;
                    Vector3 w2s = ESP.mainCam.WorldToScreenPoint(head);

                    // If they're outside of our FOV.
                    if (Vector2.Distance(new Vector2(Screen.width / 2, Screen.height / 2), new Vector2(w2s.x, w2s.y)) > 120f)
                        continue;

                    ztarget = zombie;
                }

            foreach (EntityPlayer player in O.PlayerList) {
                if (player && player.IsAlive() && player != O.localPlayer) {
                    Vector3 head = player.emodel.GetHeadTransform().position;
                    Vector3 w2s = ESP.mainCam.WorldToScreenPoint(head);

                    if (Vector2.Distance(new Vector2(Screen.width / 2, Screen.height / 2), new Vector2(w2s.x, w2s.y)) > 120f)
                        continue;

                    pTarget = player;
                }
            }

            if (pTarget) {
                // Purposely not giving the damage source an ID, so servers can't track you for killing people.
                DamageSource source = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Concuss);

                ztarget.DamageEntity(source, 100, false, 1f);
                ztarget.AwardKill(O.localPlayer);
            }

            if (ztarget) {
                DamageSource source = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Concuss);
                source.CreatorEntityId = O.localPlayer.entityId;

                ztarget.DamageEntity(source, 100, false, 1f);
                ztarget.AwardKill(O.localPlayer);

                O.localPlayer.AddKillXP(ztarget);
            }
        }

        private void Update() {
            /*if (!Input.anyKey || !Input.anyKeyDown) {
                return;
            }*/

            if (noWeaponBob && O.localPlayer) {
                vp_FPWeapon weapon = O.localPlayer.vp_FPWeapon;

                if (weapon) {
                    weapon.BobRate = Vector4.zero;
                    weapon.ShakeAmplitude = Vector3.zero;
                    weapon.RenderingFieldOfView = 120f;
                    weapon.StepForceScale = 0f;
                }
            }

            if (Input.GetKeyDown(KeyCode.O)) {
                if (!O.localPlayer) {
                    return;
                }

                Inventory inventory = O.localPlayer.inventory;

                if (inventory != null) {
                    ItemActionAttack gun = inventory.GetHoldingGun();

                    if (gun != null) {
                        gun.InfiniteAmmo = !gun.InfiniteAmmo;
                    }
                }
            }

            if (Input.GetKey(KeyCode.LeftAlt) && magicBullet) {
                MagicBullet();
            }

            if (Input.GetKey(KeyCode.LeftAlt) && O.zombieList.Count > 0 && aimbot) {
                Aimbot();
            }

            if (Input.GetKeyDown(KeyCode.F2)) {
                speed = !speed;

                Time.timeScale = speed ? 6f : 1f;
            }

            if (Time.time >= lastChamTime && chams) {
                foreach (Entity entity in FindObjectsOfType<Entity>()) {
                    if (!entity) {
                        continue;
                    }

                    switch (entity.entityType) {
                        case EntityType.Zombie:
                            ApplyChams(entity, Color.red);
                            break;
                        case EntityType.Player:
                            ApplyChams(entity, Color.cyan);
                            break;
                        case EntityType.Animal:
                            ApplyChams(entity, Color.yellow);
                            break;
                        case EntityType.Unknown:
                            ApplyChams(entity, Color.white);
                            break;
                    }
                }

                lastChamTime = Time.time + 10f;
            }
        }

        private void ApplyChams(Entity entity, Color color) {
            foreach (Renderer renderer in entity.GetComponentsInChildren<Renderer>()) {
                renderer.material = chamsMaterial;
                renderer.material.SetColor(_Color, color);
            }
        }

        private int _Color;

        private float lastChamTime;

        private Material chamsMaterial;

        public static bool speed, aimbot, infiniteAmmo, noWeaponBob, magicBullet, chams = false;
    }
}
