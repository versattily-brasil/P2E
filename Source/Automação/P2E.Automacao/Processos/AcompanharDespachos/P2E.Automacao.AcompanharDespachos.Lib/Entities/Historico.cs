﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2E.Automacao.AcompanharDespachos.Lib.Entities
{
    public class Historico
    {
        public int CD_HIST { get; set; }
        public string TX_NUM_DEC { get; set; }
        public string TX_STATUS { get; set; }
        public string TX_CANAL { get; set; }
        public DateTime DT_DATA { get; set; }
        public DateTime HR_HORA { get; set; }
    }
}
