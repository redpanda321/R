﻿/* Copyright 2013-2016 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;
using NUnit.Framework;

namespace MongoDB.Driver.Core.Operations
{
    [TestFixture]
    public class ListIndexesUsingCommandOperationTests : OperationTestBase
    {
        // test methods
        [Test]
        public void CollectionNamespace_get_should_return_expected_result()
        {
            var subject = new ListIndexesUsingCommandOperation(_collectionNamespace, _messageEncoderSettings);

            var result = subject.CollectionNamespace;

            result.Should().BeSameAs(_collectionNamespace);
        }

        [Test]
        public void constructor_should_initialize_subject()
        {
            var subject = new ListIndexesUsingCommandOperation(_collectionNamespace, _messageEncoderSettings);

            subject.CollectionNamespace.Should().BeSameAs(_collectionNamespace);
            subject.MessageEncoderSettings.Should().BeSameAs(_messageEncoderSettings);
        }

        [Test]
        public void constructor_should_throw_when_collectionNamespace_is_null()
        {
            Action action = () => new ListIndexesUsingCommandOperation(null, _messageEncoderSettings);

            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("collectionNamespace");
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.0.0")]
        public void Execute_should_return_expected_result(
            [Values(false, true)]
            bool async)
        {
            EnsureCollectionExists(async);
            var subject = new ListIndexesUsingCommandOperation(_collectionNamespace, _messageEncoderSettings);

            var result = ExecuteOperation(subject, async);
            var list = ReadCursorToEnd(result, async);

            list.Select(index => index["name"].AsString).Should().BeEquivalentTo("_id_");
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.0.0")]
        public void Execute_should_return_expected_result_when_collection_does_not_exist(
            [Values(false, true)]
            bool async)
        {
            DropCollection(async);
            var subject = new ListIndexesUsingCommandOperation(_collectionNamespace, _messageEncoderSettings);

            var result = ExecuteOperation(subject, async);
            var list = ReadCursorToEnd(result, async);

            list.Count.Should().Be(0);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.0.0")]
        public void Execute_should_return_expected_result_when_database_does_not_exist(
            [Values(false, true)]
            bool async)
        {
            DropDatabase(async);
            var subject = new ListIndexesUsingCommandOperation(_collectionNamespace, _messageEncoderSettings);

            var result = ExecuteOperation(subject, async);
            var list = ReadCursorToEnd(result, async);

            list.Count.Should().Be(0);
        }

        [Test]
        public void Execute_should_throw_when_binding_is_null(
            [Values(false, true)]
            bool async)
        {
            var subject = new ListIndexesUsingCommandOperation(_collectionNamespace, _messageEncoderSettings);
            IReadBinding binding = null;

            Action action = () => ExecuteOperation(subject, binding, async);

            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("binding");
        }

        [Test]
        public void MessageEncoderSettings_get_should_return_expected_result()
        {
            var subject = new ListIndexesUsingCommandOperation(_collectionNamespace, _messageEncoderSettings);

            var result = subject.MessageEncoderSettings;

            result.Should().BeSameAs(_messageEncoderSettings);
        }

        // helper methods
        public void DropCollection(bool async)
        {
            var operation = new DropCollectionOperation(_collectionNamespace, _messageEncoderSettings);
            ExecuteOperation(operation, async);
        }

        public void DropDatabase(bool async)
        {
            var operation = new DropDatabaseOperation(_collectionNamespace.DatabaseNamespace, _messageEncoderSettings);
            ExecuteOperation(operation, async);
        }

        public void EnsureCollectionExists(bool async)
        {
            var requests = new[] { new InsertRequest(new BsonDocument("_id", ObjectId.GenerateNewId())) };
            var operation = new BulkInsertOperation(_collectionNamespace, requests, _messageEncoderSettings);
            ExecuteOperation(operation, async);
        }
    }
}
