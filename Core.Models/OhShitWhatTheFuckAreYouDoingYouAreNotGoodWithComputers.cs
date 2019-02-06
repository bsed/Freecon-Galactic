using System;

namespace Freecon.Models
{
    /// <summary>
    /// Exception which may be encountered while CUI (Coding under the influence)
    /// [Encountered a state which shouldn't be possible]
    /// </summary>
    public class OhShitWhatTheFuckAreYouDoingYouAreNotGoodWithComputers : Exception
    {
        public OhShitWhatTheFuckAreYouDoingYouAreNotGoodWithComputers()
        { }

        public OhShitWhatTheFuckAreYouDoingYouAreNotGoodWithComputers(string message):base(message)
        {

        }


    }
}
