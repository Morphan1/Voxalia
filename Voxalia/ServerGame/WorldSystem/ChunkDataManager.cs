//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteDB;
using Voxalia.Shared;
using Voxalia.Shared.Collision;
using Voxalia.Shared.Files;

namespace Voxalia.ServerGame.WorldSystem
{
    public class ChunkDataManager
    {
        public Region TheRegion;

        LiteDatabase Database;

        LiteCollection<BsonDocument> DBChunks;

        LiteDatabase LODsDatabase;

        LiteCollection<BsonDocument> DBLODs;

        LiteDatabase EntsDatabase;

        LiteCollection<BsonDocument> DBEnts;

        LiteDatabase ImageDatabase;

        LiteCollection<BsonDocument> DBImages;

        LiteCollection<BsonDocument> DBMaxes;

        LiteCollection<BsonDocument> DBImages2;
        
        public void Init(Region tregion)
        {
            TheRegion = tregion;
            string dir = "/saves/" + TheRegion.TheWorld.Name + "/";
            TheRegion.TheServer.Files.CreateDirectory(dir);
            dir = TheRegion.TheServer.Files.BaseDirectory + dir;
            Database = new LiteDatabase("filename=" + dir + "chunks.ldb");
            DBChunks = Database.GetCollection<BsonDocument>("chunks");
            LODsDatabase = new LiteDatabase("filename=" + dir + "lod_chunks.ldb");
            DBLODs = LODsDatabase.GetCollection<BsonDocument>("lodchunks");
            EntsDatabase = new LiteDatabase("filename=" + dir + "ents.ldb");
            DBEnts = EntsDatabase.GetCollection<BsonDocument>("ents");
            ImageDatabase = new LiteDatabase("filename=" + dir + "images.ldb");
            DBImages = ImageDatabase.GetCollection<BsonDocument>("images");
            DBMaxes = ImageDatabase.GetCollection<BsonDocument>("maxes");
            DBImages2 = ImageDatabase.GetCollection<BsonDocument>("images_angle");
        }

        public KeyValuePair<int, int> GetMaxes(int x, int y)
        {
            BsonDocument doc;
            doc = DBMaxes.FindById(GetIDFor(x, y, 0));
            if (doc == null)
            {
                return new KeyValuePair<int, int>(0, 0);
            }
            return new KeyValuePair<int, int>(doc["min"].AsInt32, doc["max"].AsInt32);
        }

        public void SetMaxes(int x, int y, int min, int max)
        {
            BsonValue id = GetIDFor(x, y, 0);
            BsonDocument newdoc = new BsonDocument();
            Dictionary<string, BsonValue> tbs = newdoc.RawValue;
            tbs["_id"] = id;
            tbs["min"] = new BsonValue(min);
            tbs["max"] = new BsonValue(max);
            if (!DBMaxes.Update(newdoc))
            {
                DBMaxes.Insert(newdoc);
            }
        }

        public byte[] GetImageAngle(int x, int y, int z)
        {
            BsonDocument doc;
            doc = DBImages2.FindById(GetIDFor(x, y, z));
            if (doc == null)
            {
                return null;
            }
            return doc["image"].AsBinary;
        }

        public byte[] GetImage(int x, int y, int z)
        {
            BsonDocument doc;
            doc = DBImages.FindById(GetIDFor(x, y, z));
            if (doc == null)
            {
                return null;
            }
            return doc["image"].AsBinary;
        }

        public void WriteImageAngle(int x, int y, int z, byte[] data)
        {
            BsonValue id = GetIDFor(x, y, z);
            BsonDocument newdoc = new BsonDocument();
            Dictionary<string, BsonValue> tbs = newdoc.RawValue;
            tbs["_id"] = id;
            tbs["image"] = new BsonValue(data);
            if (!DBImages2.Update(newdoc))
            {
                DBImages2.Insert(newdoc);
            }
        }

        public void WriteImage(int x, int y, int z, byte[] data)
        {
            BsonValue id = GetIDFor(x, y, z);
            BsonDocument newdoc = new BsonDocument();
            Dictionary<string, BsonValue> tbs = newdoc.RawValue;
            tbs["_id"] = id;
            tbs["image"] = new BsonValue(data);
            if (!DBImages.Update(newdoc))
            {
                DBImages.Insert(newdoc);
            }
        }

        public void Shutdown()
        {
            Database.Dispose();
            LODsDatabase.Dispose();
            EntsDatabase.Dispose();
            ImageDatabase.Dispose();
        }

        public BsonValue GetIDFor(int x, int y, int z)
        {
            byte[] array = new byte[12];
            Utilities.IntToBytes(x).CopyTo(array, 0);
            Utilities.IntToBytes(y).CopyTo(array, 4);
            Utilities.IntToBytes(z).CopyTo(array, 8);
            return new BsonValue(array);
        }

        public byte[] GetLODChunkDetails(int x, int y, int z)
        {
            BsonDocument doc;
            doc = DBLODs.FindById(GetIDFor(x, y, z));
            if (doc == null)
            {
                return null;
            }
            return FileHandler.Uncompress(doc["blocks"].AsBinary);
        }

        public void WriteLODChunkDetails(int x, int y, int z, byte[] LOD)
        {
            BsonValue id = GetIDFor(x, y, z);
            BsonDocument newdoc = new BsonDocument();
            Dictionary<string, BsonValue> tbs = newdoc.RawValue;
            tbs["_id"] = id;
            tbs["blocks"] = new BsonValue(FileHandler.Compress(LOD));
            DBLODs.Delete(id);
            DBLODs.Insert(newdoc);
            if (!DBLODs.Update(newdoc))
            {
                DBLODs.Insert(newdoc);
            }
        }

        public ChunkDetails GetChunkEntities(int x, int y, int z)
        {
            BsonDocument doc;
            doc = DBEnts.FindById(GetIDFor(x, y, z));
            if (doc == null)
            {
                return null;
            }
            ChunkDetails det = new ChunkDetails();
            det.X = x;
            det.Y = y;
            det.Z = z;
            det.Version = doc["version"].AsInt32;
            det.Blocks = /*FileHandler.UnGZip(*/doc["entities"].AsBinary/*)*/;
            return det;
        }

        public void WriteChunkEntities(ChunkDetails details)
        {
            BsonValue id = GetIDFor(details.X, details.Y, details.Z);
            BsonDocument newdoc = new BsonDocument();
            Dictionary<string, BsonValue> tbs = newdoc.RawValue;
            tbs["_id"] = id;
            tbs["version"] = new BsonValue(details.Version);
            tbs["entities"] = new BsonValue(/*FileHandler.GZip(*/details.Blocks/*)*/);
            if (!DBEnts.Update(newdoc))
            {
                DBEnts.Insert(newdoc);
            }
        }

        public ChunkDetails GetChunkDetails(int x, int y, int z)
        {
            BsonDocument doc;
            doc = DBChunks.FindById(GetIDFor(x, y, z));
            if (doc == null)
            {
                return null;
            }
            ChunkDetails det = new ChunkDetails();
            det.X = x;
            det.Y = y;
            det.Z = z;
            det.Version = doc["version"].AsInt32;
            det.Flags = (ChunkFlags)doc["flags"].AsInt32;
            det.Blocks = FileHandler.Uncompress(doc["blocks"].AsBinary);
            det.Reachables = doc["reach"].AsBinary;
            return det;
        }

        public void WriteChunkDetails(ChunkDetails details)
        {
            BsonValue id = GetIDFor(details.X, details.Y, details.Z);
            BsonDocument newdoc = new BsonDocument();
            Dictionary<string, BsonValue> tbs = newdoc.RawValue;
            tbs["_id"] = id;
            tbs["version"] = new BsonValue(details.Version);
            tbs["flags"] = new BsonValue((int)details.Flags);
            tbs["blocks"] = new BsonValue(FileHandler.Compress(details.Blocks));
            tbs["reach"] = new BsonValue(details.Reachables);
            if (!DBChunks.Update(newdoc))
            {
                DBChunks.Insert(newdoc);
            }
        }

        public void ClearChunkDetails(Vector3i details)
        {
            BsonValue id = GetIDFor(details.X, details.Y, details.Z);
            DBChunks.Delete(id);
        }
    }
}
