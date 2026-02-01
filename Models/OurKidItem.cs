using System;
using System.Collections.Generic;

namespace WebTheMasseysEvents.Models
{
    public class OurKidItem
    {
        public string Slug { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Partner { get; set; }
        public string? Location { get; set; }
        public int Order { get; set; } = 100;

        public string? Cover { get; set; }
        public List<string> HomePhotos { get; set; } = new();

        public string? KidsLink { get; set; }
        public string? Blurb { get; set; }

        public string BodyMarkdown { get; set; } = "";

        public DateOnly? DateOfBirth { get; set; }

        public int? AgeToday
        {
            get
            {
                if (DateOfBirth is null) return null;

                var today = DateOnly.FromDateTime(DateTime.Today);
                var age = today.Year - DateOfBirth.Value.Year;

                if (today < DateOfBirth.Value.AddYears(age))
                    age--;

                return age;
            }
        }
    }
}
