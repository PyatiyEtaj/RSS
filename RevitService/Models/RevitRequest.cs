using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RevitService.Models
{
    public class RevitRequest
    {
        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [BsonElement("requestType")]
        public string RequestType { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("date")]
        public DateTime Date { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("documentPath")]
        public string DocumentPath { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }

        [BsonElement("key")]
        public Guid Key { get; set; }

        [BsonElement("serverResponse")]
        public string ServerResponse { get; set; }
    }
}
