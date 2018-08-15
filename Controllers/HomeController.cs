using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Belt.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Belt.Controllers
{
    public class HomeController : Controller
    {
        private Context _context;
        public HomeController(Context context){
            _context = context;
        }

        public IActionResult Index(){
            HttpContext.Session.Clear();
            return View();
        }

        [HttpGet]
        [Route("Register")]
        public IActionResult Reg(){
            HttpContext.Session.Clear();
            return View("Register");
        }

        [HttpPost]
        [Route("Register")]
        public IActionResult Register(RegisterUser user){
            if(ModelState.IsValid){
                PasswordHasher<RegisterUser> Hasher = new PasswordHasher<RegisterUser>();
                user.Password = Hasher.HashPassword(user, user.Password);

                User _User = new User();
                _User.FirstName = user.FirstName;
                _User.LastName = user.LastName;
                _User.UserName = user.UserName;
                _User.Password = user.Password;
                _User.Wallet = 1000.00;
                _User.TotalBid = 0.00;

                _context.Add(_User);
                _context.SaveChanges();
                HttpContext.Session.SetInt32("Id", _User.Id);
                Console.WriteLine(HttpContext.Session.GetInt32("Id"));
                return RedirectToAction("Dashboard");
            }
            else {
                return View("Register");
            }
        }

        public IActionResult Login(string username, string password){
            User _User = _context.users.SingleOrDefault(user => user.UserName == username);
            if(_User != null){
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                if(0 != Hasher.VerifyHashedPassword(_User, _User.Password, password)){
                    HttpContext.Session.SetInt32("Id", _User.Id);
                    return RedirectToAction("Dashboard");
                }
                else {
                    return View("Index");
                }
            }
            else {
                ViewBag.Errors = "Invalid username or password.";
                return View("Index");
            }
        }

        [HttpGet("Dashboard")]
        public IActionResult Dashboard(){
            if(HttpContext.Session.GetInt32("Id") != null){
                User _User = _context.users.SingleOrDefault(u => u.Id == HttpContext.Session.GetInt32("Id"));
                ViewBag.user = _User;
                List<Auction> auctions = _context.auctions.Include(u => u.User).Include(l=> l.Bids).ThenInclude(u=>u.User).OrderBy(c => c.EndDate.Date).ToList();
                
                foreach(var auction in auctions){
                    var time = (auction.EndDate - DateTime.Now);
                    if(time.Days == 0 && time.Hours == 0 && time.Minutes == 0){
                        auction.User.Wallet += auction.Bids[0].bid;
                        auction.Bids[0].User.Wallet -= auction.Bids[0].bid;
                        auction.Bids[0].User.TotalBid -= auction.Bids[0].bid;
                        _context.Remove(auction);
                        _context.SaveChanges();
                    }
                }

                return View("Dashboard", auctions);
                
            }
            else {
                return View("Index");
            }
        }

        [HttpGet("NewAuction")]
        public IActionResult NewAuction(){
            if(HttpContext.Session.GetInt32("Id") != null){
                return View("NewAuction");
            }
            else {
                return View("Index");
            }
        }

        [HttpPost("CreateNewAuction")]
        public IActionResult CreateNewAuction(Auction auction){
            if(HttpContext.Session.GetInt32("Id") != null){
                if(ModelState.IsValid){
                    if(auction.EndDate > DateTime.Now){
                        System.Console.WriteLine(auction);
                        System.Console.WriteLine(auction);
                        Auction NewAuction = new Auction();
                        NewAuction.userid = (int)HttpContext.Session.GetInt32("Id");
                        NewAuction.Name = auction.Name;
                        NewAuction.Description = auction.Description;
                        NewAuction.StartingBid = auction.StartingBid;
                        NewAuction.EndDate = auction.EndDate;

                        _context.Add(NewAuction);
                        _context.SaveChanges();
                        return RedirectToAction("Dashboard");
                    }
                    else {
                        ViewBag.Errors = "Your end date must be in the future!";
                        return View("NewAuction");
                    }
                }
                else{
                    ViewBag.Errors = "Your end date must be in the future.";
                    return View("NewAuction");
                }
            }
            else {
                return View("Index");
            }
        }

        [HttpGet("Auction/{Id}")]
        public IActionResult Auction(int Id){
            if(HttpContext.Session.GetInt32("Id") != null){
                Auction auction = _context.auctions.Include(u => u.User).Include(L => L.Bids).ThenInclude(U => U.User).SingleOrDefault(a => a.Id == Id);
                return View("Auction", auction);
            }
            else {
                return View("Index");
            }
        }

        [HttpPost("NewBid")]
        public IActionResult NewBid(double newbid, int Id){
            if(HttpContext.Session.GetInt32("Id") != null){
                User _User = _context.users.SingleOrDefault(u => u.Id == HttpContext.Session.GetInt32("Id"));
                Auction _Auction = _context.auctions.SingleOrDefault(a => a.Id == Id);
                var bidcheck = _User.TotalBid;
                if(newbid <= _User.Wallet && (bidcheck += newbid) <= _User.Wallet  && newbid > _Auction.StartingBid){
                    //create and add user to bid
                    Bid _bid = new Bid();
                    _bid.bid = newbid;
                    _bid.userid = _User.Id;
                    _bid.auctionid = Id;
                    _context.bids.Add(_bid);
                    //add to user's totalbid pool
                    _User.TotalBid += newbid;
                    //change auction's startingbid to the new bid
                    _Auction.StartingBid = newbid;
                    _context.SaveChanges();
                    return RedirectToAction("Auction", new {Id = Id});
                }
                else if(newbid <= _Auction.StartingBid){
                    TempData["Error"] = "Your bid isn't high enough.";
                    return RedirectToAction("Auction", new {Id = Id});
                }
                else {
                    TempData["Error"] = "You don't have enough money in your wallet.";
                    return RedirectToAction("Auction", new {Id = Id});
                }
            }
            else {
                return View("Index");
            }
        }

        [HttpGet("Delete/{Id}")]
        public IActionResult Delete(int Id){
            if(HttpContext.Session.GetInt32("Id") != null){
                Auction auction = _context.auctions.Include(u => u.User).Include(L => L.Bids).ThenInclude(U => U.User).SingleOrDefault(a => a.Id == Id);
                _context.auctions.Remove(auction);
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            else {
                return View("Index");
            }
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
    
}
