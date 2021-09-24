using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RevitService.Models
{
    public class KsDocument
    {
        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [BsonElement("start")]
        public DateTime Start { get; set; }

        [BsonElement("end")]
        public DateTime End { get; set; }

        [BsonElement("filepath")]
        public string FilePath { get; set; }

        [BsonElement("filepathchanged")]
        public string FilePathChanged { get; set; }

        [BsonElement("roles")]
        public string[] Roles { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("isapproved")]
        public bool IsApproved { get; set; }

        [BsonElement("urn")]
        public string Urn { get; set; }
        [BsonElement("hubId")]
        public string HubId { get; set; }

        [BsonElement("projectId")]
        public string ProjectId { get; set; }
    }
}
