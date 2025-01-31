using Framework.Code.Infrastructure.Services.Assets;
using Framework.Code.Infrastructure.Services.PersistentProgress;
using UnityEngine;
using Zenject;

namespace Framework.Code.Factories.Levels
{
	public class LevelFactory : ILevelFactory
	{
		private readonly DiContainer _diContainer;
		private readonly IPersistentProgressService _progressService;
		private readonly IAssetProvider _assetProvider;

		private string[] _tutorialLevels;
		private string[] _levels;
		private LevelDataBase _levelDataBase;

		public Level CurrentLevel { get; private set; }

		public LevelFactory(DiContainer diContainer, IPersistentProgressService progressService,
			IAssetProvider assetProvider)
		{
			_diContainer = diContainer;
			_progressService = progressService;
			_assetProvider = assetProvider;
		}

		public void Load()
		{
			_levelDataBase = _assetProvider.Load<LevelDataBase>(AssetPath.LEVELS_DATABASE);
			_tutorialLevels = _levelDataBase.TutorialLevels;
			_levels = _levelDataBase.Levels;
		}

		public Level Create()
		{
			if (CurrentLevel == null)
			{
				var existingLevel = Object.FindObjectOfType<Level>();
				if (existingLevel != null)
				{
					Debug.Log("Level already in scene");
			
					CurrentLevel = existingLevel;
					return CurrentLevel;
				}
			}
			
			if (_levels.Length == 0 && _tutorialLevels.Length == 0)
			{
				Debug.Log("No levels loaded");
				return null;
			}
			

			Level level = LoadCurrentLevel();

			CurrentLevel =
				_diContainer.InstantiatePrefabForComponent<Level>(level, Vector3.zero, Quaternion.identity, null);
			return CurrentLevel;
		}

		private Level LoadCurrentLevel()
		{
			Level level;
			if (_tutorialLevels != null && _progressService.Progress.Level <= _tutorialLevels.Length)
			{
				level = _assetProvider.Load<Level>(
					$"{AssetPath.TUTORIAL_LEVELS}/{_tutorialLevels[_progressService.Progress.Level - 1]}");
			}
			else
			{
				int index =
					(_progressService.Progress.Level - (_tutorialLevels != null ? _tutorialLevels.Length + 1 : 1)) %
					_levels.Length;

				level = _assetProvider.Load<Level>($"{AssetPath.LEVELS}/{_levels[index]}");
			}

			return level;
		}
	}
}