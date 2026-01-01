using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ControlledSublimation
{
	[JsonObject(MemberSerialization.OptIn)]
	[ConfigFile(POptions.CONFIG_FILE_NAME, false, true)]
	[RestartRequired]
	public sealed class ControlledSublimationOptions
	{
		// Singleton instance for easy access
		private static ControlledSublimationOptions _instance;
		public static ControlledSublimationOptions Instance
		{
			get
			{
				if (_instance == null)
					_instance = POptions.ReadSettings<ControlledSublimationOptions>() ?? new ControlledSublimationOptions();
				return _instance;
			}
		}

		// ========== BLEACH STONE (Chlorine) ==========
		[Option("Bleach Stone - As Debris", "Allow Bleach Stone to sublimate when lying as debris (not in storage)", "Bleach Stone → Chlorine")]
		[JsonProperty]
		public bool BleachStoneDebris { get; set; } = true;

		[Option("Bleach Stone - In Storage", "Allow Bleach Stone to sublimate when stored in containers", "Bleach Stone → Chlorine")]
		[JsonProperty]
		public bool BleachStoneInStorage { get; set; } = true;

		[Option("Bleach Stone - Rate Multiplier", "Multiplier for sublimation rate (0.0 = disabled, 1.0 = normal, 2.0 = double)", "Bleach Stone → Chlorine")]
		[Limit(0.0, 10.0)]
		[JsonProperty]
		public float BleachStoneMultiplier { get; set; } = 1.0f;

		// ========== OXYLITE (Oxygen) ==========
		[Option("Oxylite - As Debris", "Allow Oxylite to sublimate when lying as debris (not in storage)", "Oxylite → Oxygen")]
		[JsonProperty]
		public bool OxyliteDebris { get; set; } = true;

		[Option("Oxylite - In Storage", "Allow Oxylite to sublimate when stored in containers", "Oxylite → Oxygen")]
		[JsonProperty]
		public bool OxyliteInStorage { get; set; } = true;

		[Option("Oxylite - Rate Multiplier", "Multiplier for sublimation rate (0.0 = disabled, 1.0 = normal, 2.0 = double)", "Oxylite → Oxygen")]
		[Limit(0.0, 10.0)]
		[JsonProperty]
		public float OxyliteMultiplier { get; set; } = 1.0f;

		// ========== POLLUTED DIRT (Polluted Oxygen) ==========
		[Option("Polluted Dirt - As Debris", "Allow Polluted Dirt to off-gas when lying as debris", "Polluted Dirt → Polluted Oxygen")]
		[JsonProperty]
		public bool PollutedDirtDebris { get; set; } = true;

		[Option("Polluted Dirt - In Storage", "Allow Polluted Dirt to off-gas when stored in containers", "Polluted Dirt → Polluted Oxygen")]
		[JsonProperty]
		public bool PollutedDirtInStorage { get; set; } = true;

		[Option("Polluted Dirt - Rate Multiplier", "Multiplier for off-gassing rate (0.0 = disabled, 1.0 = normal, 2.0 = double)", "Polluted Dirt → Polluted Oxygen")]
		[Limit(0.0, 10.0)]
		[JsonProperty]
		public float PollutedDirtMultiplier { get; set; } = 1.0f;

		// ========== SLIME (Polluted Oxygen) ==========
		[Option("Slime - As Debris", "Allow Slime to off-gas when lying as debris", "Slime → Polluted Oxygen")]
		[JsonProperty]
		public bool SlimeDebris { get; set; } = true;

		[Option("Slime - In Storage", "Allow Slime to off-gas when stored in containers", "Slime → Polluted Oxygen")]
		[JsonProperty]
		public bool SlimeInStorage { get; set; } = true;

		[Option("Slime - Rate Multiplier", "Multiplier for off-gassing rate (0.0 = disabled, 1.0 = normal, 2.0 = double)", "Slime → Polluted Oxygen")]
		[Limit(0.0, 10.0)]
		[JsonProperty]
		public float SlimeMultiplier { get; set; } = 1.0f;

		// ========== POLLUTED WATER (Polluted Oxygen) ==========
		[Option("Polluted Water - As Debris", "Allow Polluted Water bottles to off-gas when lying as debris", "Polluted Water → Polluted Oxygen")]
		[JsonProperty]
		public bool PollutedWaterDebris { get; set; } = true;

		[Option("Polluted Water - In Storage", "Allow Polluted Water bottles to off-gas when stored in containers", "Polluted Water → Polluted Oxygen")]
		[JsonProperty]
		public bool PollutedWaterInStorage { get; set; } = true;

		[Option("Polluted Water - Rate Multiplier", "Multiplier for off-gassing rate (0.0 = disabled, 1.0 = normal, 2.0 = double)", "Polluted Water → Polluted Oxygen")]
		[Limit(0.0, 10.0)]
		[JsonProperty]
		public float PollutedWaterMultiplier { get; set; } = 1.0f;

		// ========== POLLUTED MUD (Polluted Oxygen) - Spaced Out DLC ==========
		[Option("Polluted Mud - As Debris", "Allow Polluted Mud to off-gas when lying as debris (Spaced Out DLC)", "Polluted Mud → Polluted Oxygen (DLC)")]
		[JsonProperty]
		public bool PollutedMudDebris { get; set; } = true;

		[Option("Polluted Mud - In Storage", "Allow Polluted Mud to off-gas when stored in containers (Spaced Out DLC)", "Polluted Mud → Polluted Oxygen (DLC)")]
		[JsonProperty]
		public bool PollutedMudInStorage { get; set; } = true;

		[Option("Polluted Mud - Rate Multiplier", "Multiplier for off-gassing rate (0.0 = disabled, 1.0 = normal, 2.0 = double)", "Polluted Mud → Polluted Oxygen (DLC)")]
		[Limit(0.0, 10.0)]
		[JsonProperty]
		public float PollutedMudMultiplier { get; set; } = 1.0f;

	}

	// Helper struct to hold element settings
	public struct ElementSettings
	{
		public bool AllowAsDebris;
		public bool AllowInStorage;
		public float RateMultiplier;

		public ElementSettings(bool debris, bool storage, float multiplier)
		{
			AllowAsDebris = debris;
			AllowInStorage = storage;
			RateMultiplier = multiplier;
		}
	}
}
