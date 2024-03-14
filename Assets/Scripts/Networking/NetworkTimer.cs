using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NetworkTimer
{
    public int Tick { get; set; }
    public void Update()
    {
        Tick++;
    }
}
