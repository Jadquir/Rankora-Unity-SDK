using UnityEngine;
using UnityEngine.UI;

namespace Rankora_API.Scripts.UI.Other
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class RoundedCorners : MonoBehaviour
    {
        static readonly string FullRoundedCorner = "Rankora API/UI/RoundedTexture_Full"; // Path inside Resources folder (without extension)
        static readonly string TopRoundedCorner = "Rankora API/UI/RoundedTexture_Top"; 
        static readonly string BottomRoundedCorner = "Rankora API/UI/RoundedTexture_Bottom";

        public enum RoundedCornerPlacementType
        {
            Full = 0,
            Top, 
            Bottom
        }

        [Header("Rounded Corners Settings")]
        public RoundedCornerPlacementType RoundedCornerPlacement = RoundedCornerPlacementType.Full;

        [Range(0, 256)] public float radius = 40f;

        private Image image;
        private Sprite runtimeSpriteInstance; // Local copy to avoid modifying shared sprite

        private void OnEnable()
        {
            image = GetComponent<Image>();
            LoadAndApply();
        }

        private void OnValidate()
        {
            if (image == null) image = GetComponent<Image>();
            LoadAndApply();
        }

        private void OnDisable()
        {
            // Clean up runtime sprite instance to avoid memory leaks
            if (runtimeSpriteInstance != null)
            {
                DestroyImmediate(runtimeSpriteInstance);
                runtimeSpriteInstance = null;
            }
        }
        bool TryLoad(string path, out Sprite sprite)
        {
            sprite = null;
            try
            {
                sprite = Resources.Load<Sprite>(path);
                return sprite != null;
            }
            catch (System.Exception) { }
            return false;
        }
        Sprite LoadSprite()
        {
            Sprite sprite = null;
            if(RoundedCornerPlacement == RoundedCornerPlacementType.Top && TryLoad(TopRoundedCorner, out sprite))
            {
                return sprite;
            }
            if(RoundedCornerPlacement == RoundedCornerPlacementType.Bottom && TryLoad(BottomRoundedCorner, out sprite))
            {
                return sprite;
            }
            TryLoad(FullRoundedCorner, out sprite);
            return sprite;
            
        }
        private void LoadAndApply()
        {
            if(radius == 0)
            {
                image.sprite = null;
                return;
            }
            // Choose custom or fallback sprite
            Sprite sourceSprite = LoadSprite();
            if (sourceSprite == null)
            {
                if (Application.isPlaying) {
					Debug.LogWarning($"RoundedCorners: Could not find sprite at Resources/{FullRoundedCorner}");
				}
                return;
            }

            // If we already created a runtime copy, destroy it before making a new one
            if (runtimeSpriteInstance != null)
            {
                DestroyImmediate(runtimeSpriteInstance);
                runtimeSpriteInstance = null;
            }

            // Create a runtime copy so slicing/ppu changes won’t affect the original asset
            runtimeSpriteInstance = Sprite.Create(
                sourceSprite.texture,
                sourceSprite.rect,
                sourceSprite.pivot,
                sourceSprite.pixelsPerUnit,
                0, 
                SpriteMeshType.FullRect, 
                sourceSprite.border
            );

            // Apply to Image
            image.sprite = runtimeSpriteInstance;
            image.type = Image.Type.Sliced;
            image.preserveAspect = true;

            // Adjust pixels per unit scaling relative to radius
            image.pixelsPerUnitMultiplier = radius == 0 ? 1 : 256f / radius;
        }
    }
}
