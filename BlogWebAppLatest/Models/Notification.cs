using BlogWebApp.Models.IdentityModel;
using System.ComponentModel.DataAnnotations;

namespace BlogWebApp.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }   // This is the Notification Title
        public string? Body { get; set; }   
        public string? MessageType { get; set; }
        public string? Username { get; set; }
        public Guid UserId { get; set; }
        public string? ForUserId { get; set; }
        // Foreign key referencing the user who receives the notification
        //public int? ReactionId { get; set; } // Nullable foreign key referencing the reaction associated with the notification
        public Guid BlogId { get; set; } // Foreign key referencing the blog associated with the notification
        //public int? CommentId { get; set; } // Nullable foreign key referencing the comment associated with the notification
        public bool IsRead { get; set; } // Indicates whether the notification has been read
        public DateTime CreatedAt { get; set; } // Timestamp indicating when the notification was created
        public DateTime UpdatedAt { get; set; } // Timestamp indicating when the notification was created

        // Navigation property to access the user who receives the notification
        public User User { get; set; }

        // Navigation property to access the reaction associated with the notification (if any)
        //public Reaction Reaction { get; set; }

        // Navigation property to access the comment associated with the notification (if any)
        //public Comment Comment { get; set; }
        // Navigation property to access the blog associated with the notification
        public Blog Blog { get; set; }
    
}

}
