namespace CsvIndexer
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Manages indexes for a file
    /// </summary>
    public class Indexer
    {
        /// <summary>
        /// The file to be indexed
        /// </summary>
        private readonly string fileToIndex;

        /// <summary>
        /// Delimiter for splitting the csv line
        /// </summary>
        private readonly char[] csvDelimiter = new char[] { ',' };

        /// <summary>
        /// Delimiter for splitting the index line
        /// </summary>
        private readonly char[] indexDelimiter = new char[] { '|' };

        /// <summary>
        /// file extension
        /// </summary>
        private readonly string extension = "fidx";

        /// <summary>
        /// The file index
        /// </summary>
        private Dictionary<string, IList<int[]>> index = new Dictionary<string, IList<int[]>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Indexer"/> class.
        /// </summary>
        /// <param name="fileToIndex">Index this file.</param>
        public Indexer(string fileToIndex) : this(fileToIndex, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Indexer"/> class.
        /// </summary>
        /// <param name="fileToIndex">Index of the file to.</param>
        /// <param name="hasHeader">if set to <c>true</c> [has header].</param>
        public Indexer(string fileToIndex, bool hasHeader)
        {
            this.HasHeader = hasHeader;
            this.fileToIndex = fileToIndex;          
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has header.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has header; otherwise, <c>false</c>.
        /// </value>
        public bool HasHeader { get; set; }

        /// <summary>
        /// Creates the index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="columnToIndex">Column to index.</param>
        public void Create(string indexName, int columnToIndex)
        {
            string indexFileName = this.GetIndexFileName(indexName);
            using (var indexFile = System.IO.File.CreateText(indexFileName))
            {                                
                using (var reader = new StreamReader(this.fileToIndex))
                {
                    int offset = 0;
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var columns = line.Split(this.csvDelimiter);
                        if (columns.Length - 1 >= columnToIndex)
                        {
                            var key = columns[columnToIndex];
                            key = key.Replace('"', ' ').Trim();

                            if (this.index.ContainsKey(key))
                            {
                                this.index[key].Add(new int[] { offset, line.Length });
                            }
                            else
                            {
                                this.index.Add(key, new List<int[]>() { new int[] { offset, line.Length } });                                
                            }
                        }

                        offset += line.Length + 2;
                    }
                }

                foreach (var key in this.index.Keys)
                {
                    indexFile.Write(string.Format("{0}{1}", key, this.indexDelimiter[0]));
                    foreach (var item in this.index[key])
                    {
                        indexFile.Write(string.Format("{0}{1}{2}{3}", item[0], this.csvDelimiter[0], item[1], this.indexDelimiter[0]));
                    }

                    indexFile.Write(string.Format("\n"));
                }
            }
        }

        /// <summary>
        /// Loads the specified index name.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        public void Load(string indexName)
        {
            string indexFileName = this.GetIndexFileName(indexName);
            using (var indexFile = System.IO.File.OpenText(indexFileName))
            {
                this.index.Clear();

                string line;
                while ((line = indexFile.ReadLine()) != null)
                {
                    var ix = line.Split(this.indexDelimiter);
                    for (int i = 0; i < ix.Length; i++)
                    {
                        if (i == 0)
                        {
                            this.index.Add(ix[0], new List<int[]>());
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(ix[i]))
                            {
                                continue;
                            }

                            var parts = ix[i].Split(this.csvDelimiter);
                            this.index[ix[0]].Add(new int[] { Convert.ToInt32(parts[0]), Convert.ToInt32(parts[1]) });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Seeks the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// The list of row offsets , line lengths.
        /// </returns>
        public IList<int[]> Seek(string key)
        {
            IList<int[]> result = new List<int[]>();

            if (this.index.ContainsKey(key))
            {
                foreach (var parts in this.index[key])
                {
                    result.Add(new int[] { parts[0], parts[1] });   
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the name of the index file.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <returns>The name of the index file.</returns>
        private string GetIndexFileName(string indexName)
        {
            return string.Format("{0}.{1}.{2}", System.IO.Path.GetFileNameWithoutExtension(this.fileToIndex), indexName, this.extension);
        }
    }
}
