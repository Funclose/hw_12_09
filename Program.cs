using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using static HW_12_09.Program;

namespace HW_12_09
{
    class Program
    {
        public enum GuestRole
        {
            VIP,
            Attendee,
            Organizer,
            Speaker
        }
        static void Main(string[] args)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                EventService value = new EventService(db);
                var events = new List<Event>()
                {
                    new Event{Name = "Birthday" },
                    new Event{Name = "New_Year" },
                    new Event{Name = "1 September"}
                };

                var GuestName = new List<Guest>()
                {
                    new Guest{Name = "John Smith" },
                    new Guest{Name = "Albert Horish" },
                    new Guest{Name = "Jany Jave" },
                    new Guest{Name = "Sebastian Velld" }
                };
                var relationGuest = new List<EventGuestRelation>()
                {
                    new EventGuestRelation{Events = events[1], Guest = GuestName[1], Role = GuestRole.Attendee },
                    new EventGuestRelation{Events = events[0], Guest = GuestName[2], Role = GuestRole.VIP },
                    new EventGuestRelation{Events = events[2], Guest = GuestName[1], Role = GuestRole.Speaker }
                };
                db.Events.AddRange(events);
                db.Guests.AddRange(GuestName);
                db.EventGuestRelations.AddRange(relationGuest);

                var guestIdCheck = 2;

                //3 метода 

                value.GetALLEventGuestSpeaker(guestIdCheck).ToList();
                value.ChangeRoleGuest(0, 0, GuestRole.VIP);
                value.RemoveGuest(0);
                db.SaveChanges();
            }
        }
        



        public class Guest
        { 
            public int Id { get; set; }
            public string Name { get; set; }
            
            public ICollection<EventGuestRelation> EventGuestRelations { get; set; }
        }
        public class Event
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ICollection<EventGuestRelation> EventGuestRelations { get; set; }
            
        }

        public class EventGuestRelation
        { 
            public int GuestId { get; set; }
            public Guest Guest { get; set; }
            public int EventId { get; set; }
            public Event Events { get; set; }

            public GuestRole Role { get; set; }
        }

        public class ApplicationContext : DbContext
        {
            public DbSet<Event> Events { get; set; }
            public DbSet<Guest> Guests { get; set; }
            public DbSet<EventGuestRelation> EventGuestRelations { get; set; }
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
                optionsBuilder.UseSqlServer("Server=DESKTOP-TBASQVJ;Database=testdb;Trusted_Connection=True;TrustServerCertificate=True;");
            }
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {

                modelBuilder.Entity<EventGuestRelation>()
                    .HasKey(e => new { e.EventId, e.GuestId });

                modelBuilder.Entity<EventGuestRelation>()
                    .HasOne(egr => egr.Guest)
                    .WithMany(g => g.EventGuestRelations)
                    .HasForeignKey(egr => egr.GuestId);

                modelBuilder.Entity<EventGuestRelation>()
                    .HasOne(egr => egr.Events)
                    .WithMany(e => e.EventGuestRelations)
                    .HasForeignKey(egr => egr.EventId);

                modelBuilder.Entity<EventGuestRelation>()
                    .Property(egr => egr.Role)
                    .HasDefaultValue(GuestRole.Attendee);
            }
        }






        public class EventService
        {
            private readonly ApplicationContext _context;
            public EventService(ApplicationContext context)
            {
                _context = context;
            }

            public void ChangeRoleGuest(int eventId, int guestId,GuestRole newrole)
            {
                var Change = _context.EventGuestRelations
                    .FirstOrDefault(e => e.EventId == eventId && e.GuestId == guestId);
                if (Change != null)
                {
                    Change.Role = newrole;
                    _context.SaveChanges();
                }
                throw new NotImplementedException();
            }
            public List<Event> GetAllEventsFromUser(int guestId)
            {
                var getAll = _context.EventGuestRelations
                    .Where(e => e.GuestId == guestId)
                    .Select(eg => eg.Events)
                    .ToList();
                return getAll;
            }

            public List<Event> GetALLEventGuestSpeaker(int guestId)
            {
                var getGuest = _context.EventGuestRelations
                    .Where(e => e.GuestId == guestId && e.Role == GuestRole.Speaker)
                    .Select(eg => eg.Events)
                    .ToList();
                return getGuest;
            }
            public void RemoveGuest(int guestId)
            {
                var guest = _context.EventGuestRelations
                    .SingleOrDefault(e => e.GuestId == guestId);
                if (guest != null)
                {
                    _context.EventGuestRelations.Remove(guest);
                }
                throw new Exception("user not found");
                    
                    
            }
        }



    }
}
