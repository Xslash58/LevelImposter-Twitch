﻿using HarmonyLib;
using LevelImposter.Core;
using System;
using TMPro;
using UnityEngine;

namespace LevelImposter.Shop
{
    /*
     *      I swear to god if I find that
     *      any of you patch over my logo
     *      I will swallow your entire house whole.
     */
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public static class VersionPatch
    {
        private static Sprite? _logoSprite = null;
        private static GameObject? _versionObject = null;

        public static GameObject? VersionObject => _versionObject;

        public static void Postfix()
        {
            if (!GameState.IsInMainMenu)
                return;

            string antiPiracy = Guid.NewGuid().ToString();

            _versionObject = new("LevelImposterVersion " + antiPiracy);
            _versionObject.transform.localScale = new Vector3(0.55f, 0.55f, 1.0f);
            _versionObject.layer = (int)Layer.UI;

            AspectPosition logoPosition = _versionObject.AddComponent<AspectPosition>();
            logoPosition.Alignment = AspectPosition.EdgeAlignments.Right;
            logoPosition.DistanceFromEdge = new Vector3(1.8f, -2.3f, 0);
            logoPosition.AdjustPosition();

            SpriteRenderer logoRenderer = _versionObject.AddComponent<SpriteRenderer>();
            logoRenderer.sprite = GetLogoSprite();

            GameObject logoTextObj = new("LevelImposterText " + antiPiracy);
            logoTextObj.transform.SetParent(_versionObject.transform);
            logoTextObj.transform.localPosition = new Vector3(3.2f, 0, 0);

            RectTransform logoTransform = logoTextObj.AddComponent<RectTransform>();
            logoTransform.sizeDelta = new Vector2(2, 0.19f);

            TextMeshPro logoText = logoTextObj.AddComponent<TextMeshPro>();
            logoText.fontSize = 1.5f;
            logoText.alignment = TextAlignmentOptions.BottomLeft;
            logoText.raycastTarget = false;
            logoText.SetText("v" + LevelImposter.DisplayVersion);
        }

        private static Sprite GetLogoSprite()
        {
            if (_logoSprite == null)
                _logoSprite = MapUtils.LoadSpriteResource("LevelImposterLogo.png");
            if (_logoSprite == null)
                throw new Exception("The \"LevelImposterLogo.png\" resource was not found in assembly");
            return _logoSprite;
        }
    }
}
