using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MainMenuExpansion
{
	[KSPAddon(KSPAddon.Startup.MainMenu, false)]
	public class MainMenuExpansion : MonoBehaviour
	{
		MainMenu menu;
		MainMenuEnvLogic env;
		Transform msdTarget;

		void Start()
		{
			menu = MainMenu.FindObjectOfType<MainMenu> ();
			//menu.SpaceportURL = "http://www.kerbalstuff.com"; //hehehe

			env = menu.envLogic;
			bool isMun = env.areas [0].activeInHierarchy == true;
			//set it to space thingy
			env.areas [0].SetActive(false);
			env.areas [1].SetActive(true);

			GameObject orbitArea = env.areas [1];

			//remove kerbin
			var kerbin = orbitArea.transform.Find ("Kerbin");
			kerbin.gameObject.SetActive (false);

			//remove mun
			var munPivot = orbitArea.transform.Find ("MunPivot").gameObject;
			munPivot.SetActive (false);

			msdTarget = new GameObject ("MSD_Target").transform;
			msdTarget.position = new Vector3 (1400, 650, 0);

			//get a random body to do
			var r = new System.Random ();
			int count = PSystemManager.Instance.systemPrefab.rootBody.children.Where(p => p.scaledVersion.renderer.material.shader.name != "Emissive Multi Ramp Sunspots").ToArray().Length;
			PSystemBody main = PSystemManager.Instance.systemPrefab.rootBody.children.Where(p => p.scaledVersion.renderer.material.shader.name != "Emissive Multi Ramp Sunspots").ToArray()[r.Next(0, count - 1)];

			//build the body
			Debug.Log ("building " + main.name);
			var mainPlanet = MakeMenuPlanet (main, orbitArea, true).transform;

			//build it's moons
			foreach (var child in main.children)
			{
				Debug.Log ("building " + child.name);
				MakeMenuMoon (child, mainPlanet.gameObject, orbitArea);
			}
		}

		public GameObject MakeMenuPlanet(PSystemBody body, GameObject area, bool isMain)
		{
			GameObject planet = (GameObject)Instantiate (body.scaledVersion);
			planet.transform.parent = area.transform;
			planet.transform.localScale *= 7f;
			if (isMain)
			{
				planet.transform.position = new Vector3 (700f, -100f, 2000f) *  (float)(body.celestialBody.Radius / 600000.0);
			}
			planet.gameObject.layer = 0;

			//kill the fancy scaled crap
			DestroyImmediate(planet.GetComponent<ScaledSpaceFader> ());
			DestroyImmediate(planet.GetComponent<SphereCollider> ());

			DestroyImmediate(planet.GetComponentInChildren<AtmosphereFromGround> ());
			var msd = planet.GetComponent<MaterialSetDirection> ();
			if (msd != null)
				msd.target = msdTarget;

			var rotato = planet.AddComponent<Rotato> ();
			rotato.speed = -0.005f / (float)(body.celestialBody.rotationPeriod / 400.0);

			return planet;
		}
		public GameObject MakeMenuMoon(PSystemBody body, GameObject MenuPlanet, GameObject area)
		{
			GameObject pivot = new GameObject ("MoonPivot");
			pivot.gameObject.layer = 0;
			pivot.transform.position = MenuPlanet.transform.position;
			var rotato = pivot.AddComponent<Rotato> ();
			rotato.speed = -0.012f * (float)(body.orbitDriver.orbit.getOrbitalSpeedAtDistance (body.orbitDriver.orbit.semiMajorAxis) / 542.5);

			var planet = MakeMenuPlanet (body, area, false);
			planet.transform.parent = pivot.transform;
			planet.transform.localPosition = new Vector3 (-5000f * (float)(body.orbitDriver.orbit.semiMajorAxis / 12000000.0), 0f, 0f); //12000000 is semi major axis of mun

			//apply orbital crap
			pivot.transform.Rotate (0f, (float)body.orbitDriver.orbit.LAN, 0f);
			pivot.transform.Rotate (0f, 0f, (float)body.orbitDriver.orbit.inclination);
			pivot.transform.Rotate (0f, (float)body.orbitDriver.orbit.argumentOfPeriapsis, 0f);

			planet.gameObject.layer = 0;
			return pivot;
		}
	}
}

