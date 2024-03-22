using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface Lerpable<T>
{
    public abstract T Lerp(T b, T t);
}
