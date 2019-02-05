using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AtlasManager {
    private List<AtlasTexture> iconAtlasList;
    private AtlasTexture latestTexture;
    public AtlasManager () {
        iconAtlasList = new List<AtlasTexture> ();
    }
    public struct AtlasInfo : IEquatable<AtlasInfo> {
        public int atlasId;
        public int textureId;
        public Texture2D packedTexture;
        public Rect uvRect;

        public override bool Equals (object other) {
            if (!(other is AtlasInfo)) return false;
            return Equals ((AtlasInfo) other);
        }

        public bool Equals (AtlasInfo other) {
            return atlasId.Equals (other.atlasId) && textureId.Equals (other.textureId) && packedTexture.Equals (other.packedTexture) && uvRect.Equals (other.uvRect);
        }

        public override int GetHashCode () {
            return atlasId.GetHashCode () ^ (textureId.GetHashCode () << 2) ^ packedTexture.GetHashCode () ^ uvRect.GetHashCode ();
        }
    }

    public AtlasInfo AddIconTextureToAtlas (Texture2D tex) {
        if (iconAtlasList.Count == 0) {
            latestTexture = new AtlasTexture (1, 88);
            iconAtlasList.Add (latestTexture);
        } else if (iconAtlasList.Single (a => a.atlasId == iconAtlasList.Count).textureCount >= 100) {
            latestTexture = new AtlasTexture (iconAtlasList.Count + 1, 88);
            iconAtlasList.Add (latestTexture);
        }
        //AtlasTexture tmp = iconAtlasList.Single (a => a.atlasId == iconAtlasList.Count);
        AtlasInfo info = new AtlasInfo ();
        info.atlasId = iconAtlasList.Count;
        info.textureId = latestTexture.textureCount;
        info.uvRect = latestTexture.AddTexture (tex);
        info.packedTexture = latestTexture.packedTexture;
        return info;
    }
}

public class AtlasTexture {
    public int atlasId;
    public int textureCount;
    private int textureSquareSize;
    public Texture2D packedTexture;
    private Texture2D[] textures;
    private Rect[] UVRect;
    private int padding = 2;
    public AtlasTexture (int id, int texSize) {
        atlasId = id;
        textureCount = 0;
        textureSquareSize = texSize;
        packedTexture = new Texture2D (1024, 1024);
        textures = new Texture2D[105];
        for (int i = 0; i < textures.Length; i++) {
            textures[i] = new Texture2D (texSize, texSize, TextureFormat.DXT1, false);
        }
    }

    public Rect AddTexture (Texture2D tex) {
        tex.Compress (false);
        textures[textureCount] = tex;
        textureCount++;
        UVRect = packedTexture.PackTextures (textures, padding, textureSquareSize * 10);
        return UVRect[textureCount - 1];
    }
}