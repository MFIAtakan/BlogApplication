﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogLab.Models.BlogComment
{
    public class BlogComment : BlogCommentCreate
    {
        public string Username { get; set; }
        public int ApplicationUserId { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime UpdateDate { get; set; }

    }
}
