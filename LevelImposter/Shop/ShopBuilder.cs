﻿using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LevelImposter.Core;
using LevelImposter.DB;

namespace LevelImposter.Shop
{
    public static class ShopBuilder
    {
        private static GameObject mapShopPrefab = null;

        public static void OnLoad()
        {
            RemoveChildren();
            GameObject shopSpawner = new GameObject("Shop Spawner");
            shopSpawner.AddComponent<ShopSpawner>();
        }

        public static GameObject GetShopPrefab()
        {
            if (mapShopPrefab == null)
                mapShopPrefab = MapUtils.LoadAssetBundle("shop");
            return mapShopPrefab;
        }

        private static void RemoveChildren()
        {
            GameObject controller = GameObject.Find("HowToPlayController");
            controller.transform.FindChild("IntroScene").gameObject.active = false;
            controller.transform.FindChild("RightArrow").gameObject.active = false;
            controller.transform.FindChild("Dots").gameObject.active = false;
        }

        public static GameObject BuildShop()
        {
            GameObject mapShop = Object.Instantiate(GetShopPrefab());

            ShopManager shopMgr = mapShop.AddComponent<ShopManager>();
            shopMgr.shopParent = mapShop.transform.FindChild("Canvas").FindChild("Scroll").FindChild("Viewport").FindChild("Content");
            shopMgr.mapBannerPrefab = shopMgr.shopParent.FindChild("MapBanner").gameObject.AddComponent<MapBanner>();

            ShopButtons shopBtns = mapShop.AddComponent<ShopButtons>();
            Transform btnsParent = mapShop.transform.FindChild("Canvas").FindChild("Shop Buttons");
            shopBtns.downloadedButton = btnsParent.FindChild("DownloadedBtn").GetComponent<Button>();
            shopBtns.topButton = btnsParent.FindChild("TopBtn").GetComponent<Button>();
            shopBtns.recentButton = btnsParent.FindChild("RecentBtn").GetComponent<Button>();
            shopBtns.featuredButton = btnsParent.FindChild("FeaturedBtn").GetComponent<Button>();
            shopBtns.folderButton = btnsParent.FindChild("FolderBtn").GetComponent<Button>();

            MapBanner bannerPrefab = shopMgr.mapBannerPrefab;
            bannerPrefab.transform.FindChild("LoadOverlay").FindChild("LoadingSpinner").gameObject.AddComponent<Spinner>();

            mapShop.transform.FindChild("Canvas").FindChild("CloseBtn").GetComponent<Button>().onClick.AddListener((System.Action)ShopManager.CloseShop);

            Transform starField = mapShop.transform.FindChild("Star Field");
            StarGen starGen = starField.gameObject.AddComponent<StarGen>();
            starGen.Length = 12;
            starGen.Width = 6;
            MeshRenderer starRenderer = starField.gameObject.GetComponent<MeshRenderer>();

            Transform skeld = AssetDB.ss["ss-skeld"].ShipStatus.transform;
            starRenderer.material = skeld.FindChild("starfield").gameObject.GetComponent<MeshRenderer>().material;

            return mapShop;
        }
    }
}