namespace ApiAggregator.Models
{
    public class RepositoryModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Owner Owner { get; set; }
    }

    public class Owner
    {
       
        public string Url { get; set; }
       
    }

   
}
