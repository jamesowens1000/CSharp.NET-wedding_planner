using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    public class MyContext : DbContext
    {
        // base() calls the parent class' constructor passing the "options" parameter along
        public MyContext(DbContextOptions options) : base(options) { }
        public DbSet<User> Users {get;set;}
        public DbSet<Wedding> Weddings {get;set;}
        public DbSet<Attendance> Attendances {get;set;}
    }

    public class LoginUser
    {
        [Required]
        [EmailAddress]
        public string Email {get;set;}

        [Required]
        [DataType(DataType.Password)]
        public string Password {get;set;}
    }

    public class User
    {
        [Key]
        public int UserId {get;set;}

        [Required]
        [Display(Name = "First Name")]
        public string FirstName {get;set;}

        [Required]
        [Display(Name = "Last Name")]
        public string LastName {get;set;}

        [Required]
        [EmailAddress]
        public string Email {get;set;}

        [Required]
        [MinLength(8, ErrorMessage="Password must be 8 characters or longer!")]
        [DataType(DataType.Password)]
        public string Password {get;set;}

        public List<Attendance> AttendedWeddings {get;set;} //The list of Weddings that a User is attending
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;

        [NotMapped]
        [Compare("Password")]
        [DataType(DataType.Password)]
        [Display(Name = "PW Confirm")]
        public string Confirm {get;set;}
    }

    public class Wedding
    {
        [Key]
        public int WeddingId {get;set;}

        [Required]
        [Display(Name = "Wedder One")]
        public string WedderOne {get;set;}

        [Required]
        [Display(Name = "Wedder Two")]
        public string WedderTwo {get;set;}

        [Required]
        [FutureDate]
        [Display(Name = "Date")]
        public DateTime WeddDate {get;set;}

        [Required]
        [Display(Name = "Wedding Address")]
        public string WeddAddress {get;set;}
        public int PlannerId {get;set;} //The UserId of the User (in session) who created the Wedding
        public List<Attendance> WeddingAttendees {get;set;} //The list of Users who are attending this Wedding
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
    }

    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (((DateTime)value) <= DateTime.Today)
            {
                return new ValidationResult("Only dates in the future are allowed!");
            }
            return ValidationResult.Success;
        }
    }

    public class Attendance
    {
        [Key]
        public int AttendanceId {get;set;}
        public int UserId {get;set;}
        public int WeddingId {get;set;}
        public User User {get;set;}
        public Wedding Wedding {get;set;}
    }

    public class IndexViewModel
    {
        public LoginUser LogUser {get;set;}
        public User RegUser {get;set;}
    }
}