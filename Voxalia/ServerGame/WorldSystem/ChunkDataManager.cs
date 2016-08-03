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
        
        public Object FSLock = new Object();

        public Object EntLock = new Object();

        public Object LODLock = new Object();

        public Object IMGLock = new Object();

        public void Init(Region tregion)
        {
            TheRegion = tregion;
            Database = new LiteDatabase("filename=" + TheRegion.TheServer.Files.BaseDirectory + "/saves/" + TheRegion.Name + "/chunks.ldb");
            DBChunks = Database.GetCollection<BsonDocument>("chunks");
            LODsDatabase = new LiteDatabase("filename=" + TheRegion.TheServer.Files.BaseDirectory + "/saves/" + TheRegion.Name + "/lod_chunks.ldb");
            DBLODs = LODsDatabase.GetCollection<BsonDocument>("lodchunks");
            EntsDatabase = new LiteDatabase("filename=" + TheRegion.TheServer.Files.BaseDirectory + "/saves/" + TheRegion.Name + "/ents.ldb");
            DBEnts = EntsDatabase.GetCollection<BsonDocument>("ents");
            ImageDatabase = new LiteDatabase("filename=" + TheRegion.TheServer.Files.BaseDirectory + "/saves/" + TheRegion.Name + "/images.ldb");
            DBImages = ImageDatabase.GetCollection<BsonDocument>("images");
            DBMaxes = ImageDatabase.GetCollection<BsonDocument>("maxes");
            DBImages2 = ImageDatabase.GetCollection<BsonDocument>("images_angle");
        }
        
        public KeyValuePair<int, int> GetMaxes(int x, int y)
        {
            BsonDocument doc;
            lock (IMGLock)
            {
                doc = DBMaxes.FindById(GetIDFor(x, y, 0));
            }
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
            newdoc["_id"] = id;
            newdoc["min"] = new BsonValue(min);
            newdoc["max"] = new BsonValue(max);
            lock (IMGLock)
            {
                DBMaxes.Delete(id);
                DBMaxes.Insert(newdoc);
            }
        }

        public byte[] GetImageAngle(int x, int y, int z)
        {
            BsonDocument doc;
            lock (IMGLock)
            {
                doc = DBImages2.FindById(GetIDFor(x, y, z));
            }
            if (doc == null)
            {
                return null;
            }
            return doc["image"].AsBinary;
        }

        public byte[] GetImage(int x, int y, int z)
        {
            BsonDocument doc;
            lock (IMGLock)
            {
                doc = DBImages.FindById(GetIDFor(x, y, z));
            }
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
            newdoc["_id"] = id;
            newdoc["image"] = new BsonValue(data);
            lock (IMGLock)
            {
                DBImages2.Delete(id);
                DBImages2.Insert(newdoc);
            }
        }

        public void WriteImage(int x, int y, int z, byte[] data)
        {
            BsonValue id = GetIDFor(x, y, z);
            BsonDocument newdoc = new BsonDocument();
            newdoc["_id"] = id;
            newdoc["image"] = new BsonValue(data);
            lock (IMGLock)
            {
                DBImages.Delete(id);
                DBImages.Insert(newdoc);
            }
        }

        public void Shutdown()
        {
            Database.Dispose();
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
            lock (LODLock)
            {
                doc = DBLODs.FindById(GetIDFor(x, y, z));
            }
            if (doc == null)
            {
                return null;
            }
            return FileHandler.UnGZip(doc["blocks"].AsBinary);
        }

        public void WriteLODChunkDetails(int x, int y, int z, byte[] LOD)
        {
            BsonValue id = GetIDFor(x, y, z);
            BsonDocument newdoc = new BsonDocument();
            newdoc["_id"] = id;
            newdoc["blocks"] = new BsonValue(FileHandler.GZip(LOD));
            lock (LODLock)
            {
                DBLODs.Delete(id);
                DBLODs.Insert(newdoc);
            }
        }

        public ChunkDetails GetChunkEntities(int x, int y, int z)
        {
            BsonDocument doc;
            lock (EntLock)
            {
                doc = DBEnts.FindById(GetIDFor(x, y, z));
            }
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
            newdoc["_id"] = id;
            newdoc["version"] = new BsonValue(details.Version);
            newdoc["entities"] = new BsonValue(/*FileHandler.GZip(*/details.Blocks/*)*/);
            lock (EntLock)
            {
                DBEnts.Delete(id);
                DBEnts.Insert(newdoc);
            }
        }

        public ChunkDetails GetChunkDetails(int x, int y, int z)
        {
            BsonDocument doc;
            lock (FSLock)
            {
                doc = DBChunks.FindById(GetIDFor(x, y, z));
            }
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
            det.Blocks = FileHandler.UnGZip(doc["blocks"].AsBinary);
            det.Reachables = doc["reach"].AsBinary;
            return det;
        }
        
        public void WriteChunkDetails(ChunkDetails details)
        {
            BsonValue id = GetIDFor(details.X, details.Y, details.Z);
            BsonDocument newdoc = new BsonDocument();
            newdoc["_id"] = id;
            newdoc["version"] = new BsonValue(details.Version);
            newdoc["flags"] = new BsonValue((int)details.Flags);
            newdoc["blocks"] = new BsonValue(FileHandler.GZip(details.Blocks));
            newdoc["reach"] = new BsonValue(details.Reachables);
            lock (FSLock)
            {
                DBChunks.Delete(id);
                DBChunks.Insert(newdoc);
            }
        }

        public void ClearChunkDetails(Vector3i details)
        {
            BsonValue id = GetIDFor(details.X, details.Y, details.Z);
            lock (FSLock)
            {
                DBChunks.Delete(id);
            }
        }
    }
}
