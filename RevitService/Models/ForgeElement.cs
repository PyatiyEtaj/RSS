using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace RevitService.Models
{
    public class ForgeElement
    {
        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [BsonElement("dbid")]
        public int DbId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("approved")]
        public bool Approved { get; set; }

        [BsonElement("s")]
        public string Section { get; set; }

        [BsonElement("f")]
        public string Floor { get; set; }

        [BsonElement("c")]
        public string Construction { get; set; }

        [BsonElement("m")]
        public string Material { get; set; }

        [BsonElement("cm")]
        public string CharMat { get; set; }

        [BsonElement("p")]
        public string Params { get; set; }

        [BsonElement("square")]
        public double? Square{ get; set; }

        [BsonElement("volume")]
        public double? Volume { get; set; }

        [BsonElement("date")]
        public DateTime Date { get; set; }

        [BsonElement("dateapproved")]
        public DateTime DateApproved { get; set; }

        [BsonElement("revitid")]
        public int RevitId { get; set; }


        public override string ToString()
            => $"dbid: {DbId}\nname: {Name}\napproved:{Approved}\n" +
            $"s: {Section}\nf: {Floor}\nc: {Construction}\nm: {Material}\n" +
            $"cm: {CharMat}\np: {Params}\nsquare: {Square}\nvolume: {Volume}\n" +
            $"date: {Date}\nrevitid: {RevitId}\ndateapproved: {DateApproved}";
    }
}
