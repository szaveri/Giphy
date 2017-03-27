using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace Giphy
{
    public class HttpRequest
    {
        /*
        * Generic function to get list of GIFs based on various perameters
        */
        public static async Task<RootObject> GetQuery(Uri uri)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(uri);
                var result = await response.Content.ReadAsStringAsync();
                var serializer = new DataContractJsonSerializer(typeof(RootObject));

                var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
                var data = (RootObject)serializer.ReadObject(ms);

                return data;
            }
        }

        public static Uri GenerateURL(string type, int offset, string query)
        {
            string url = "http://api.giphy.com/v1/gifs/" + type + "?";

            if (query != null)
            {
                url += "q=" + query + "&";
            }
            url += "limit=" + Global.limit + "&offset="
                    + offset + "&api_key=" + Global.GIPHY_PUBLIC_KEY;

            return new Uri(url);
        }
    }

    //JSON Object formatter
    [DataContract]
    public class FixedHeight
    {
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public string width { get; set; }
        [DataMember]
        public string height { get; set; }
        [DataMember]
        public string size { get; set; }
        [DataMember]
        public string mp4 { get; set; }
        [DataMember]
        public string mp4_size { get; set; }
        [DataMember]
        public string webp { get; set; }
        [DataMember]
        public string webp_size { get; set; }
    }
    
    [DataContract]
    public class FixedHeightDownsampled
    {
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public string width { get; set; }
        [DataMember]
        public string height { get; set; }
        [DataMember]
        public string size { get; set; }
        [DataMember]
        public string webp { get; set; }
        [DataMember]
        public string webp_size { get; set; }
    }

    [DataContract]
    public class FixedWidth
    {
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public string width { get; set; }
        [DataMember]
        public string height { get; set; }
        [DataMember]
        public string size { get; set; }
        [DataMember]
        public string mp4 { get; set; }
        [DataMember]
        public string mp4_size { get; set; }
        [DataMember]
        public string webp { get; set; }
        [DataMember]
        public string webp_size { get; set; }
    }
    
    [DataContract]
    public class FixedWidthDownsampled
    {
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public string width { get; set; }
        [DataMember]
        public string height { get; set; }
        [DataMember]
        public string size { get; set; }
        [DataMember]
        public string webp { get; set; }
        [DataMember]
        public string webp_size { get; set; }
    }
    
    [DataContract]
    public class Downsized
    {
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public string width { get; set; }
        [DataMember]
        public string height { get; set; }
        [DataMember]
        public string size { get; set; }
    }

    [DataContract]
    public class Original
    {
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public string width { get; set; }
        [DataMember]
        public string height { get; set; }
        [DataMember]
        public string size { get; set; }
        [DataMember]
        public string frames { get; set; }
        [DataMember]
        public string mp4 { get; set; }
        [DataMember]
        public string mp4_size { get; set; }
        [DataMember]
        public string webp { get; set; }
        [DataMember]
        public string webp_size { get; set; }
        [DataMember]
        public string hash { get; set; }
    }
  
    [DataContract]
    public class Images
    {
        [DataMember]
        public FixedHeight fixed_height { get; set; }
        [DataMember]
        public FixedHeightDownsampled fixed_height_downsampled { get; set; }
        [DataMember]
        public FixedWidth fixed_width { get; set; }
        [DataMember]
        public FixedWidthDownsampled fixed_width_downsampled { get; set; }
        [DataMember]
        public Downsized downsized { get; set; }
        [DataMember]
        public Original original { get; set; }
    }

    [DataContract]
    public class Datum
    {
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string slug { get; set; }
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public string bitly_gif_url { get; set; }
        [DataMember]
        public string bitly_url { get; set; }
        [DataMember]
        public string embed_url { get; set; }
        [DataMember]
        public string username { get; set; }
        [DataMember]
        public string source { get; set; }
        [DataMember]
        public string rating { get; set; }
        [DataMember]
        public string content_url { get; set; }
        [DataMember]
        public string source_tld { get; set; }
        [DataMember]
        public string source_post_url { get; set; }
        [DataMember]
        public int is_indexable { get; set; }
        [DataMember]
        public string import_datetime { get; set; }
        [DataMember]
        public string trending_datetime { get; set; }
        [DataMember]
        public Images images { get; set; }
    }

    [DataContract]
    public class Meta
    {
        [DataMember]
        public int status { get; set; }
        [DataMember]
        public string msg { get; set; }
        [DataMember]
        public string response_id { get; set; }
    }

    [DataContract]
    public class Pagination
    {
        [DataMember]
        public int total_count { get; set; }
        [DataMember]
        public int count { get; set; }
        [DataMember]
        public int offset { get; set; }
    }

    [DataContract]
    public class RootObject
    {
        [DataMember]
        public List<Datum> data { get; set; }
        [DataMember]
        public Meta meta { get; set; }
        [DataMember]
        public Pagination pagination { get; set; }
    }

}
