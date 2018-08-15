using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Belt.Models{
    public class CustomDateRangeAttribute : RangeAttribute{
    public CustomDateRangeAttribute() : base(typeof(DateTime), DateTime.Now.ToString(), DateTime.Now.AddYears(20).ToString()){}
}

    public class Auction : BaseEntity {

        [Required(ErrorMessage = "A name is required")]
        [MinLength(3, ErrorMessage="A name must be at least 3 characters long.")]
        public string Name {get;set;}

        [Required(ErrorMessage = "A description is required")]
        [MinLength(10, ErrorMessage="A description must be at least 10 characters long.")]
        public string Description {get;set;}

        [Required(ErrorMessage = "A bid must be provided")]
        [Range(0.01, Double.MaxValue, ErrorMessage="A bid cannot be less than $0.01.")]
        public double? StartingBid {get;set;}

        [Required(ErrorMessage = "You must provide a date")]
        public DateTime EndDate {get;set;}

        public int userid {get;set;}

        public User User {get;set;}
        public List<Bid> Bids {get;set;}

        public Auction(){
            Bids = new List<Bid>();
        }
    }
}