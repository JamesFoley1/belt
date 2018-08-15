using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Belt.Models{

    public class Bid : BaseEntity {
        public double bid {get;set;}
        public int userid {get;set;}
        public User User {get;set;}
        public int auctionid {get;set;}
        public Auction Auction {get;set;}
    }
}