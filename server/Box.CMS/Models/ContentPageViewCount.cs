﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Box.CMS.Models
{

    public class ContentPageViewCount {

        [Key, Column(TypeName = "char(36)"), MaxLength(36)]
        public string ContentUId { get; set; }

        public int Count { get; set; }
    }
}
