﻿// <copyright file="StorageInitializationHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.GroupBot.Common.Providers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Class handles initialization to Azure table storage.
    /// </summary>
    public class StorageInitializationHelper
    {
        /// <summary>
        /// A lazy task to initialize Azure table storage.
        /// </summary>
        protected readonly Lazy<Task> initializeTask;

        /// <summary>
        /// Azure cloud table client.
        /// </summary>
        protected CloudTableClient cloudTableClient;

        /// <summary>
        /// Cloud table for storing group activity and details regarding sending notification.
        /// </summary>
        protected CloudTable cloudTable;

        /// <summary>
        /// Azure storage table name to perform operations.
        /// </summary>
        private string tableName;

        /// <summary>
        /// Connection string of azure table storage.
        /// </summary>
        private string connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageInitializationHelper"/> class.
        /// </summary>
        /// <param name="connectionString">Connection string of azure table storage.</param>
        /// <param name="tableName">Table name of azure table storage to initialize.</param>
        public StorageInitializationHelper(string connectionString, string tableName)
        {
            this.connectionString = connectionString;
            this.tableName = tableName;
            this.initializeTask = new Lazy<Task>(() => this.InitializeAsync());
        }

        /// <summary>
        /// Ensures Microsoft Azure Table Storage should be created before working on table.
        /// </summary>
        /// <returns>Represents an asynchronous operation.</returns>
        protected async Task EnsureInitializedAsync()
        {
            await this.initializeTask.Value;
        }

        /// <summary>
        /// Create storage table if it does not exist.
        /// </summary>
        /// <param name="connectionString">Storage account connection string.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents table is created if it does not exists.</returns>
        private async Task<CloudTable> InitializeAsync()
        {
            // Exponential retry policy with back off set to 3 seconds and 5 retries.
            var exponentialRetryPolicy = new TableRequestOptions()
            {
                RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(3), 5),
            };

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.connectionString);
            this.cloudTableClient = storageAccount.CreateCloudTableClient();
            this.cloudTableClient.DefaultRequestOptions = exponentialRetryPolicy;
            this.cloudTable = this.cloudTableClient.GetTableReference(this.tableName);
            if (!await this.cloudTable.ExistsAsync())
            {
                await this.cloudTable.CreateIfNotExistsAsync();
            }

            return this.cloudTable;
        }
    }
}
