using OctoberStudio.Extensions;
using System.Collections.Generic;

namespace OctoberStudio
{
    public class RoomsShuffler
    {
        protected List<RoomData> shuffledRooms = new List<RoomData>();
        protected Dictionary<RoomData, List<WaveData>> shuffledWavesDictionary = new Dictionary<RoomData, List<WaveData>>();

        protected ContinuePlayingSave ContinuePlayingSave;

        public RoomsShuffler(StageData stageData)
        {
            ContinuePlayingSave = GameController.SaveManager.GetSave<ContinuePlayingSave>("Continue Playing");

            if (ContinuePlayingSave.HasUnfinishedStageData)
            {
                Load(stageData);
            }
            else
            {
                Shuffle(stageData);
                Save(stageData);
            }
        }

        public virtual void Shuffle(StageData stageData)
        {
            ShuffleRooms(stageData);

            shuffledWavesDictionary.Clear();
            for (int i = 0; i < shuffledRooms.Count; i++)
            {
                var roomData = shuffledRooms[i];
                var shuffledWaves = ShuffleWaves(roomData);

                shuffledWavesDictionary.Add(roomData, shuffledWaves);
            }
        }

        protected virtual void ShuffleRooms(StageData stageData)
        {
            shuffledRooms.Clear();

            var orderedRooms = new List<RoomData>(stageData.Rooms);
            var groupsRooms = new Dictionary<int, List<RoomData>>();

            for (int i = 0; i < orderedRooms.Count; i++)
            {
                var room = orderedRooms[i];
                if (room.GroupId != 0)
                {
                    if (groupsRooms.ContainsKey(room.GroupId))
                    {
                        groupsRooms[room.GroupId].Add(room);
                    }
                    else
                    {
                        groupsRooms[room.GroupId] = new List<RoomData>() { room };
                    }
                }
            }

            while (orderedRooms.Count > 0)
            {
                var room = orderedRooms[0];
                if (room.GroupId == 0)
                {
                    shuffledRooms.Add(room);
                }
                else
                {
                    var randomRoomFromGroup = groupsRooms[room.GroupId].PopRandom();
                    shuffledRooms.Add(randomRoomFromGroup);
                }

                orderedRooms.RemoveAt(0);
            }
        }

        protected virtual List<WaveData> ShuffleWaves(RoomData roomData)
        {
            var shuffledWaves = new List<WaveData>();

            var orderedWaves = new List<WaveData>(roomData.Waves);
            var groupsWaves = new Dictionary<int, List<WaveData>>();

            for (int i = 0; i < orderedWaves.Count; i++)
            {
                var wave = orderedWaves[i];
                if (wave.GroupId != 0)
                {
                    if (groupsWaves.ContainsKey(wave.GroupId))
                    {
                        groupsWaves[wave.GroupId].Add(wave);
                    }
                    else
                    {
                        groupsWaves[wave.GroupId] = new List<WaveData>() { wave };
                    }
                }
            }

            while (orderedWaves.Count > 0)
            {
                var wave = orderedWaves[0];
                if (wave.GroupId == 0)
                {
                    shuffledWaves.Add(wave);
                }
                else
                {
                    var randomWaveFromGroup = groupsWaves[wave.GroupId].PopRandom();
                    shuffledWaves.Add(randomWaveFromGroup);
                }

                orderedWaves.RemoveAt(0);
            }

            return shuffledWaves;
        }

        public virtual void Save(StageData stageData)
        {
            var shuffledRoomsIndices = new int[shuffledRooms.Count];

            for (int i = 0; i < shuffledRooms.Count; i++)
            {
                var roomData = shuffledRooms[i];
                var index = stageData.Rooms.IndexOf(roomData);

                shuffledRoomsIndices[i] = index;
            }

            var shuffledWavesIndices = new ContinuePlayingSave.ArrayWrapper[shuffledRooms.Count];

            for (int i = 0; i < shuffledRooms.Count; i++)
            {
                var roomData = shuffledRooms[i];

                var shuffledWaves = shuffledWavesDictionary[roomData];

                var indices = new int[shuffledWaves.Count];
                for (int j = 0; j < shuffledWaves.Count; j++)
                {
                    var waveData = shuffledWaves[j];
                    var index = roomData.Waves.IndexOf(waveData);

                    indices[j] = index;
                }

                shuffledWavesIndices[i] = indices;
            }

            ContinuePlayingSave.SaveShuffleData(shuffledRoomsIndices, shuffledWavesIndices);
        }

        public virtual void Load(StageData stageData)
        {
            shuffledRooms = new List<RoomData>(stageData.RoomsCount);

            for (int i = 0; i < ContinuePlayingSave.ShuffledRoomIndices.Length; i++)
            {
                var index = ContinuePlayingSave.ShuffledRoomIndices[i];
                var roomData = stageData.Rooms[index];

                shuffledRooms.Add(roomData);
            }

            shuffledWavesDictionary = new Dictionary<RoomData, List<WaveData>>();

            for (int i = 0; i < shuffledRooms.Count; i++)
            {
                var roomData = shuffledRooms[i];

                var shuffledWaves = new List<WaveData>(roomData.Waves.Length);
                var waveIndices = ContinuePlayingSave.ShuffledWaveIndices[i];

                for (int j = 0; j < waveIndices.Length; j++)
                {
                    var index = waveIndices[j];
                    var waveData = roomData.Waves[index];

                    shuffledWaves.Add(waveData);
                }

                shuffledWavesDictionary.Add(roomData, shuffledWaves);
            }
        }

        public int GetRoomIndex(RoomData roomData)
        {
            return shuffledRooms.IndexOf(roomData);
        }

        public RoomData GetRoomData(int roomIndex)
        {
            return shuffledRooms[roomIndex];
        }

        public RoomData GetActiveRoomData()
        {
            return shuffledRooms[ContinuePlayingSave.ActiveRoomId];
        }

        public RoomData GetNextRoomData()
        {
            if (ContinuePlayingSave.ActiveRoomId < shuffledRooms.Count - 1)
            {
                return shuffledRooms[ContinuePlayingSave.ActiveRoomId + 1];
            }

            return null;
        }

        public RoomData GetPrevRoomData()
        {
            if (ContinuePlayingSave.ActiveRoomId > 0)
            {
                return shuffledRooms[ContinuePlayingSave.ActiveRoomId - 1];
            }

            return null;
        }

        public WaveData GetWaveData(int waveIndex)
        {
            var activeRoomData = GetActiveRoomData();

            return shuffledWavesDictionary[activeRoomData][waveIndex];
        }

        public WaveData GetWaveData(int roomIndex, int waveIndex)
        {
            var roomData = shuffledRooms[roomIndex];

            return shuffledWavesDictionary[roomData][waveIndex];
        }

        public WaveData GetActiveWaveData()
        {
            var activeRoomData = GetActiveRoomData();

            return shuffledWavesDictionary[activeRoomData][ContinuePlayingSave.ActiveWaveId];
        }

        public WaveData GetNextWaveData()
        {
            var activeRoomData = GetActiveRoomData();

            var shuffledWaves = shuffledWavesDictionary[activeRoomData];

            if (ContinuePlayingSave.ActiveWaveId < shuffledWaves.Count - 1)
            {
                return shuffledWaves[ContinuePlayingSave.ActiveWaveId + 1];
            }

            var nextRoom = GetNextRoomData();

            if (nextRoom != null)
            {
                return shuffledWavesDictionary[nextRoom][0];
            }

            return null;
        }

        public WaveData GetNextWaveData(WaveData waveData)
        {
            var foundCounter = 0;
            for (int i = 0; i < shuffledRooms.Count; i++)
            {
                var room = shuffledRooms[i];
                var shuffledWaves = shuffledWavesDictionary[room];
                for (int j = 0; j < shuffledWaves.Count; j++)
                {
                    var wave = shuffledWaves[j];

                    if (wave == waveData || foundCounter > 0)
                    {
                        foundCounter++;
                        if (foundCounter == 2)
                        {
                            return wave;
                        }
                    }
                }
            }

            return null;
        }

        public WaveData GetPrevWaveData()
        {
            if (ContinuePlayingSave.ActiveWaveId > 0)
            {
                var activeRoomData = GetActiveRoomData();

                return shuffledWavesDictionary[activeRoomData][ContinuePlayingSave.ActiveWaveId - 1];
            }

            var prevRoom = GetPrevRoomData();

            if (prevRoom != null)
            {
                return shuffledWavesDictionary[prevRoom][^1];
            }

            return null;
        }

        public int GetRealWaveIndex(RoomData roomData, WaveData waveData)
        {
            return shuffledWavesDictionary[roomData].IndexOf(waveData);
        }

        public int GetShuffledWaveIndex(WaveData waveData)
        {
            var counter = 0;
            for (int i = 0; i < shuffledRooms.Count; i++)
            {
                var room = shuffledRooms[i];
                var shuffledWaves = shuffledWavesDictionary[room];

                var index = shuffledWaves.IndexOf(waveData);

                if (index == -1)
                {
                    counter += shuffledWaves.Count;
                }
                else
                {
                    counter += index;
                    return counter;
                }
            }

            return -1;
        }
    }
}