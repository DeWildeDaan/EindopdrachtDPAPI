using System;
using System.Collections.Generic;
using System.Text;

namespace DPEindopdrachtAPI.Models
{
    public class Comment
    {
        public Guid CommentId { get; set; }
        public int ChargerId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string CommentText { get; set; }
        public DateTime DateAndTime { get; set; }
    }
}
