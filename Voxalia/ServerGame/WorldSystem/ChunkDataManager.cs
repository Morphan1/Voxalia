using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteDB;
using Voxalia.Shared;
using Voxalia.Shared.Files;

namespace Voxalia.ServerGame.WorldSystem
{
    public class ChunkDataManager
    {
        public Region TheRegion;

        LiteDatabase Database;

        LiteCollection<BsonDocument> DBChunks;

        public Object FSLock = new Object();

        public void Init(Region tregion)
        {
            TheRegion = tregion;
            Database = new LiteDatabase("filename=" + Program.Files.BaseDirectory + "/saves/" + TheRegion.Name + "/chunks.ldb");
            DBChunks = Database.GetCollection<BsonDocument>("chunks");
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
            det.Entities = FileHandler.UnGZip(doc["entities"].AsBinary);
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
            newdoc["entities"] = new BsonValue(FileHandler.GZip(details.Entities));
            lock (FSLock)
            {
                DBChunks.Delete(id);
                DBChunks.Insert(newdoc);
            }
        }
    }
}
