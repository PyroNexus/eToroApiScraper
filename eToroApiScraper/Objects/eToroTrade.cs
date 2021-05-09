using System;
using System.Collections.Generic;
using System.Text;

namespace eToroApiScraper.Objects
{
    public class Avatars
    {
        public string small { get; set; }
        public string medium { get; set; }
        public string large { get; set; }
    }

    public class Country
    {
        public string name { get; set; }
        public string a2 { get; set; }
        public int code { get; set; }
        public int phonePrefix { get; set; }
    }

    public class User
    {
        public int accountType { get; set; }
        public int piLevel { get; set; }
        public bool isPi { get; set; }
        public int cid { get; set; }
        public int gcid { get; set; }
        public string username { get; set; }
        public object firstName { get; set; }
        public object lastName { get; set; }
        public bool allowDisplayFullName { get; set; }
        public string url { get; set; }
        public Avatars avatars { get; set; }
        public Country country { get; set; }
    }

    public class Reason
    {
        public string sourceID { get; set; }
        public User user { get; set; }
        public string type { get; set; }
    }

    public class RootData
    {
        public Reason reason { get; set; }
        public object lastUpdatedItemAt { get; set; }
        public object occurredAt { get; set; }
        public int commentsCount { get; set; }
        public int pageNumber { get; set; }
        public int totalCommentsAndRepliesCount { get; set; }
    }

    public class Images
    {
        public string _35X35 { get; set; }
        public string _50X50 { get; set; }
        public string _150X150 { get; set; }
        public string _80X80 { get; set; }
        public string _90X90 { get; set; }
        public string _70X70 { get; set; }
    }

    public class InvestmentCounter
    {
        public int investors { get; set; }
        public object updatedAt { get; set; }
    }

    public class Allocation
    {
        public int buyPercentage { get; set; }
        public int sellPercentage { get; set; }
    }

    public class Symbol
    {
        public string name { get; set; }
        public string displayName { get; set; }
        public string marketName { get; set; }
        public string categoryName { get; set; }
        public int priceFormat { get; set; }
        public string type { get; set; }
        public string typeName { get; set; }
        public int instrumentID { get; set; }
        public Images images { get; set; }
        public InvestmentCounter investmentCounter { get; set; }
        public Allocation allocation { get; set; }
    }

    public class ImageProperties
    {
        public object url { get; set; }
        public int height { get; set; }
        public int width { get; set; }
    }

    public class RichMediaData
    {
        public object title { get; set; }
        public object description { get; set; }
        public ImageProperties imageProperties { get; set; }
        public object richMediaID { get; set; }
        public string scrapDataType { get; set; }
        public object url { get; set; }
        public string host { get; set; }
    }

    public class eToroTrade
    {
        public RootData rootData { get; set; }
        public double rate { get; set; }
        public int leverage { get; set; }
        public string direction { get; set; }
        public Symbol symbol { get; set; }
        public int ratePrecision { get; set; }
        public double gain { get; set; }
        public int positionID { get; set; }
        public object messageBody { get; set; }
        public RichMediaData richMediaData { get; set; }
        public bool isShareDeleted { get; set; }
        public object tags { get; set; }
        public bool isCurrentUserModerator { get; set; }
        public object languageCode { get; set; }
        public bool isMuted { get; set; }
        public object parentID { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public object updatedAt { get; set; }
        public object occurredAt { get; set; }
        public User user { get; set; }
        public bool isFlaggedAsSpam { get; set; }
        public bool isCurrentUserFlagging { get; set; }
        public bool isCurrentUserLiking { get; set; }
        public bool isCurrentUserFollowing { get; set; }
        public bool isSavedByCurrentUser { get; set; }
        public List<object> items { get; set; }
        public int timesShared { get; set; }
        public int editStatus { get; set; }
    }
}
