﻿using Boilerpipe.Net.Extractors;
using Newtonsoft.Json;
using RedditSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace _4106Classifier {
    public class Crawler {
        public const int MinLength = 100;
        public const int MinWords = 10;
        public static readonly char[] StopMarkers = { '\n', '.', '?', '!' };
        public const string WordPattern = "[a-zA-Z]+[a-zA-Z0-9-']*";

        private Reddit reddit;

        public Crawler() {
            Settings settings = Settings.Instance;
            var agent = new BotWebAgent(
                Settings.Instance.Reddit.Username, Settings.Instance.Reddit.Password,
                Settings.Instance.Reddit.ClientID, Settings.Instance.Reddit.Secret,
                ""
            );

            reddit = new Reddit(agent, false);
        }

        /// <summary>
        /// Split a text into sentences, and words
        /// </summary>
        /// <param name="text">Input text</param>
        /// <returns>List of Lists of words</returns>
        public List<List<string>> Split(string text) {
            var sentences = new List<List<string>>();
            foreach (string sentence in text.Split(StopMarkers)) {
                List<string> words = new List<string>();
                foreach (var match in Regex.Matches(sentence.Replace('’', '\''), WordPattern)) {
                    words.Add(match.ToString());
                }

                if (words.Count > MinWords)
                    sentences.Add(words);
            }

            return sentences;
        }

        /// <summary>
        /// Fetch articles from a subreddit
        /// </summary>
        /// <param name="sub">/r/name</param>
        /// <returns>List of up to configured number of articles</returns>
        public List<Article> Articles(string sub, Article.BiasType bias) {
            List<Article> articles = new List<Article>();
            var subreddit = reddit.GetSubreddit(sub);
            while (articles.Count < Settings.Instance.Reddit.PostsToFetch) {
                int added = 0;

                var posts = subreddit.GetTop(RedditSharp.Things.FromTime.Year).Take(Settings.Instance.Reddit.PostsToFetch);
                foreach (var post in posts) {
                    if (Banned(post.Url.OriginalString)) continue;

                    Article a = GetArticle(post.Url.OriginalString, bias);
                    if (a.Sentences != null)
                        articles.Add(a);
                    added++;
                }

                if (added == 0) break;
            }

            return articles;
        }

        public List<Article> FrontPage(string sub) {
            List<Article> articles = new List<Article>();
            var subreddit = reddit.GetSubreddit(sub);
            while (articles.Count < Settings.Instance.Reddit.PostsToFetch) {
                int added = 0;

                var posts = subreddit.Hot.Take(Settings.Instance.Reddit.PostsToFetch);
                foreach (var post in posts) {
                    if (Banned(post.Url.OriginalString)) continue;

                    Article a = GetArticle(post.Url.OriginalString, Article.BiasType.Unknown);
                    if (a.Sentences != null) {
                        articles.Add(a);
                        added++;
                    }
                }

                if (added == 0) break;
            }

            return articles;
        }
        
        /// <summary>
        /// Fetch a page
        /// </summary>
        /// <param name="url">Url of the page</param>
        /// <param name="bias">Known bias of the article</param>
        /// <returns>Article</returns>
        public Article GetArticle(string url, Article.BiasType bias) {
            Article article = new Article();
            article.URL = url;
            string response = FetchPage(url);
            string text = ArticleExtractor.Instance.GetText(response);

            if (text.Length > MinLength) {
                try {
                    text = HtmlAgilityPack.HtmlEntity.DeEntitize(text);
                } catch (Exception e) { }

                article.Sentences = Split(text);
                article.Bias = bias;

            }

            return article;
        }

        /// <summary>
        /// Check if a url is allowed
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>true if url is a banned source</returns>
        public bool Banned(string url) {
            foreach (string pattern in Settings.Instance.Reddit.BannedDomains) {
                if (url.Contains(pattern)) return true;
            }

            return false;
        }

        /// <summary>
        /// Fetch a webpage
        /// </summary>
        /// <param name="url">URL of the page</param>
        /// <returns>Page HTML</returns>
        public string FetchPage(string url) {
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream)) {
                    return reader.ReadToEnd();
                }
            } catch (Exception e) {
                return "";
            }
        }

        /// <summary>
        /// A response from the mercury service
        /// </summary>
        public class MercuryResponse {
            public string error { get; set; }
            public string title { get; set; }
            public string author { get; set; }
            public string date_published { get; set; }
            public string dek { get; set; }
            public string lead_image_url { get; set; }
            public string content { get; set; }
            public string next_page_url { get; set; }
            public string domain { get; set; }
            public string excerpt { get; set; }
            public int word_count { get; set; }
            public string direction { get; set; }
            public int total_pages { get; set; }
            public int rendered_pages { get; set; }
        }
    }
}
