using System;

namespace net5Library
{
    public class Communicator
    {
        public string GetStatus(int number)
        {
            return $"Pending {number} tasks.";
        }
    }
}
