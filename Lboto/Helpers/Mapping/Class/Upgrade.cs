namespace Lboto.Helpers.Mapping
{
    public class Upgrade
    {
        
        private bool _TierEnabled;        
        private int _Tier = 1;        
        private bool _PriorityEnabled;        
        private int _Priority;

        public bool TierEnabled
        {            
            get
            {
                return _TierEnabled;
            }
            
            set
            {
                _TierEnabled = value;
            }
        }

        public int Tier
        {            
            get
            {
                return _Tier;
            }
            
            set
            {
                _Tier = value;
            }
        }

        public bool PriorityEnabled
        {            
            get
            {
                return _PriorityEnabled;
            }
            
            set
            {
                _PriorityEnabled = value;
            }
        }

        public int Priority
        {            
            get
            {
                return _Priority;
            }
            
            set
            {
                _Priority = value;
            }
        }
    }

}
