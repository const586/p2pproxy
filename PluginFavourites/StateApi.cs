namespace PluginFavourites
{
    public class StateApi
    {
        public int success;
        public ErrorType error;

        public bool IsSuccess()
        {
            return success == 1;
        }

        public string GetXState()
        {
            return "<state><success>0</success><error>" + error + "</error></state>";
        }
    }
}