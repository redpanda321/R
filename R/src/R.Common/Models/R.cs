﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization;
using System.Security.Cryptography;
using static MongoDB.Driver.WriteConcern;

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace R.Common.Models
{


    public class ErrorCode
    {

        public int Id { get; set; }
        public string Description { get; set; }

        public string LogId { get; set; }
    }


    public class Pagings
    {
        public List<Paging> pagings { get; set; }

    }


    public class Paging
    {

        public int Id { get; set; }

        public int RecordsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalRecords { get; set; }
        public int MaxRecords { get; set; }
        public int TotalPages { get; set; }
        public int RecordsShowing { get; set; }
        public int Pins { get; set; }
    }

    public class Building
    {

        public int Id { get; set; }
        public string BathroomTotal { get; set; }
        public string Bedrooms { get; set; }
        public string SizeInterior { get; set; }
        public string StoriesTotal { get; set; }
        public string Type { get; set; }
    }

    public class Address
    {
        public int Id { get; set; }
        public string AddressText { get; set; }
    }

    public class Phone
    {
        public int Id { get; set; }

        public string PhoneType { get; set; }
        public string PhoneNumber { get; set; }
        public string AreaCode { get; set; }
        public string PhoneTypeId { get; set; }
    }

    public class Email
    {
        public int Id { get; set; }
        public string ContactId { get; set; }
    }

    public class Website1
    {
        public int Id { get; set; }
        public string Website { get; set; }
        public string WebsiteTypeId { get; set; }
    }

    public class Organization
    {
        public int Id { get; set; }

        public int OrganizationID { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
        public List<Phone> Phones { get; set; }
        public List<Email> Emails { get; set; }
        public bool HasEmail { get; set; }
        public bool PermitFreetextEmail { get; set; }
        public bool PermitShowListingLink { get; set; }
        public string Logo { get; set; }
        public List<Website1> Websites { get; set; }
        public string Designation { get; set; }
    }

    public class Phone2
    {
        public int Id { get; set; }

        public string PhoneType { get; set; }
        public string PhoneNumber { get; set; }
        public string AreaCode { get; set; }
        public string PhoneTypeId { get; set; }
        public string Extension { get; set; }
    }

    public class Website2
    {
        public int Id { get; set; }
        public string Website { get; set; }
        public string WebsiteTypeId { get; set; }
    }

    public class Email2
    {
        public int Id { get; set; }
        public string ContactId { get; set; }
    }

    public class Individual
    {

        public int Id { get; set; }
        public int IndividualID { get; set; }
        public string Name { get; set; }
        public Organization Organization { get; set; }
        public List<Phone2> Phones { get; set; }
        public List<Website2> Websites { get; set; }
        public List<Email2> Emails { get; set; }
        public string Photo { get; set; }
        public string Position { get; set; }
        public bool PermitFreetextEmail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CorporationDisplayTypeId { get; set; }
        public bool? CccMember { get; set; }
        public string DesignationCodes { get; set; }
    }

    public class Address2
    {

        public int Id { get; set; }
        public string AddressText { get; set; }
        [Index]
        public double Longitude { get; set; }
        [Index]
        public double Latitude { get; set; }
    }

    public class Photo
    {
        public int Id { get; set; }
        public string SequenceId { get; set; }
        public string HighResPath { get; set; }
        public string MedResPath { get; set; }
        public string LowResPath { get; set; }
        public string LastUpdated { get; set; }
    }

    public class Parking
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Spaces { get; set; }
    }

    public class Property
    {
        public int Id { get; set; }
       
        public string Price { get; set; }
        public string Type { get; set; }
        public Address2 Address { get; set; }
        public List<Photo> Photo { get; set; }
        public List<Parking> Parking { get; set; }
        public string TypeId { get; set; }
        public string OwnershipType { get; set; }
        public string AmmenitiesNearBy { get; set; }
    }

    public class Business
    {
        public int Id { get; set; }
    }

    public class Land
    {
        public int Id { get; set; }
        public string LandscapeFeatures { get; set; }
    }

    public class AlternateURL
    {
        public int Id { get; set; }
        public string BrochureLink { get; set; }
        public string DetailsLink { get; set; }
    }


    

    public class Results {

        public List<Result> results { get; set; }

    }

    public class ResultHistory
    {


        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
       // [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        

        public DateTime ResultDateTime { get; set; }

        public string ResultId { get; set; }
        [Index]
        public float Price { get; set; }


        public Result Result { get; set; }

    }


    public class XResultHistory
    {

        public int Id { get; set; }
        public DateTime ResultDateTime { get; set; }

        public string ResultId { get; set; }
      
        public float Price { get; set; }



    }



    public class Result
    {

       [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
       // [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } 
        
        public string MlsNumber { get; set; }
        public string PublicRemarks { get; set; }

        public string PostalCode { get; set; }
      
        public string RelativeDetailsURL { get; set; }

        public Building Building { get; set; }
        public List<Individual> Individual { get; set; }
        public Property Property { get; set; }
        public Business Business { get; set; }
        public Land Land { get; set; }
       
        public AlternateURL AlternateURL { get; set; }
    }

    public class Pin
    {


       [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
       //[BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string key { get; set; }
        public string propertyId { get; set; }
        public int count { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
    }

    public class Pins {

        public List<Pin> pins { get; set; }
    }

    public class RootObject
    {
        public ErrorCode ErrorCode { get; set; }
        public Paging Paging { get; set; }
        public List<Result> Results { get; set; }
        public List<Pin> Pins { get; set; }
    }


    public class Position {

        public Position() {


             LongitudeMin = -115.90212547926488;
             LongitudeMax = -110.82094872145238;
             LatitudeMin = 50.051219586625;
             LatitudeMax = 51.523515704948416;
             Longitude = -113.914628;
             Latitude =  50.897983;


        }


        public double LongitudeMin { get; set; }
        public double LongitudeMax { get; set; }
        public double LatitudeMin { get; set; }
        public double LatitudeMax { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }



    }


}
