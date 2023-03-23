using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

[BepInPlugin(MID, PluginName, VERSION)]
[BepInProcess("valheim.exe")]
[HarmonyPatch]
public class ValheimMod : BaseUnityPlugin
{
	private const string MID = "knighty.plugins.valheim.removeyggdrasil";
	private const string VERSION = "1.0.0";
	private const string PluginName = "Remove Yggdrasil";

	ConfigFile configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "RemoveYggdrasil.cfg"), true);
	ConfigEntry<bool> showYggdrasil;

	bool lastValue = true;

	private Harmony harmony;

	IEnumerable<Transform> Objects(Transform root)
	{
		var nodeQueue = new Queue<Transform>();
		nodeQueue.Enqueue(root);
		while (nodeQueue.Count > 0)
		{
			var item = nodeQueue.Dequeue();
			foreach (Transform child in item)
			{
				nodeQueue.Enqueue(child);
			}
			yield return item;
		}
	}

	protected void Awake()
	{
		harmony = new Harmony(MID);
		harmony.PatchAll();

		showYggdrasil = configFile.Bind("Visibility", "Show Yggdrasil", false);

		SceneManager.activeSceneChanged += (a, b) => UpdateVisibility();
	}

	protected void UpdateVisibility()
	{
		foreach (GameObject gameObject in SceneManager.GetActiveScene().GetRootGameObjects())
		{
			Objects(gameObject.transform).Where(item => item.name.Contains("yggdrasil") || item.name.Contains("Yggdrasil")).Do(Debug.Log);

			Transform yggdrasil = Objects(gameObject.transform).First(item => item.name.Contains("yggdrasil") || item.name.Contains("Yggdrasil"));
			Debug.Log($"Found {yggdrasil.gameObject.name}");
			yggdrasil.gameObject.SetActive(showYggdrasil.Value);
		}
	}

	protected void OnDestroy()
	{
		harmony?.UnpatchSelf();
	}

	protected void Update()
	{
		if (lastValue != showYggdrasil.Value)
		{
			lastValue = showYggdrasil.Value;
			UpdateVisibility();
		}
	}
}
