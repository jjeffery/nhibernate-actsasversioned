using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NHibernate.ActsAsVersioned.Models;
using Xunit;

namespace NHibernate.ActsAsVersioned;

public class ActsAsVersionedTests : IDisposable
{
    private readonly List<string> _fileNames = new();

    public void Dispose()
    {
        foreach (var fileName in _fileNames)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
    }

    public string GetFileName()
    {
        var fileName = Path.Combine(Path.GetTempPath(), $"acts-as-versioned-test-{_fileNames.Count}.db");
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
        _fileNames.Add(fileName);
        return fileName;
    }
    
    [Fact]
    public void It_creates_versioned_table_rows()
    {
        TestConfiguration.DataSource = GetFileName();
        var sessionFactory = TestConfiguration.SessionFactory;
        using var session = sessionFactory.OpenSession();

        session.CreateSchema();

        var authorId = 0;

        using (var tx = session.BeginTransaction())
        {
            var author = new Author
            {
                Name = "Author 1",
                HomeAddress = new Address
                {
                    Street = "Street 1",
                    Locality = "Locality 1",
                    Postcode = "1000",
                    State = "NSW"
                }
            };

            session.Save(author);
            tx.Commit();
            authorId = author.Id;
        }

        var rowCount = CountRows(session, "author_versions");
        Assert.Equal(1, rowCount);

        using (var tx = session.BeginTransaction())
        {
            var author = session.Get<Author>(authorId);
            Assert.NotNull(author);
            session.Update(author);
            tx.Commit();
        }

        rowCount = CountRows(session, "author_versions");
        Assert.Equal(1, rowCount);

        using (var tx = session.BeginTransaction())
        {
            var author = session.Get<Author>(authorId);
            Assert.NotNull(author);
            author.HomeAddress.Postcode = "2000";
            tx.Commit();
        }
        
        rowCount = CountRows(session, "author_versions");
        Assert.Equal(2, rowCount);

        var bookId = 0;
        var expectedBookVersions = 0;

        using (var tx = session.BeginTransaction())
        {
            var author = session.Get<Author>(authorId);
            Assert.NotNull(author);

            var book = new Book
            {
                Author = author,
                Title = "Title 1"
            };

            session.Save(book);
            bookId = book.Id;
            tx.Commit();
            expectedBookVersions++;
        }

        rowCount = CountRows(session, "author_versions");
        Assert.Equal(2, rowCount);

        rowCount = CountRows(session, "book_versions");
        Assert.Equal(expectedBookVersions, rowCount);
        
        // Does not update if a non versioned column is updated
        using (var tx = session.BeginTransaction())
        {
            var book = session.Get<Book>(bookId);
            Assert.NotNull(book);
            book.NotVersioned += 1;
            tx.Commit();
        }
        
        rowCount = CountRows(session, "book_versions");
        Assert.Equal(expectedBookVersions, rowCount);
        
        using (var tx = session.BeginTransaction())
        {
            var book = session.Get<Book>(bookId);
            Assert.NotNull(book);
            book.Published = !book.Published;
            tx.Commit();
            expectedBookVersions++;
        }
        
        rowCount = CountRows(session, "book_versions");
        Assert.Equal(expectedBookVersions, rowCount);
        
        // Does not update if a non versioned column and auto update column is updated
        using (var tx = session.BeginTransaction())
        {
            var book = session.Get<Book>(bookId);
            Assert.NotNull(book);
            book.NotVersioned += 1;
            book.AutoUpdate += 1;
            tx.Commit();
        }
        
        rowCount = CountRows(session, "book_versions");
        Assert.Equal(expectedBookVersions, rowCount);

        
    }

    private int CountRows(ISession session, string tableName)
    {
        var rowCount = session.CreateSQLQuery($"select count(*) from {tableName}").List<object>().First();
        return Convert.ToInt32(rowCount);
    }
}