﻿namespace Provider.EntityFramework.Connection
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Data.SqlClient;
	using System.Linq;
	using System.Text;
	using Kernel.Data.Connection;
	using Kernel.Initialisation;

	/// <summary>
	/// Build a SQLServer connection string from an IDbConnectionDefinition
	/// </summary>
	internal class SQLServerConnectionStringBuilder : IConnectionStringProvider, IAutoRegisterAsTransient
	{
		#region static methods

		/// <summary>
		/// Builds from definiton.
		/// </summary>
		/// <param name="definition">The definition.</param>
		/// <returns></returns>
		public static SQLServerConnectionStringBuilder BuildFromDefiniton(IDbConnectionDefinition definition)
		{
			return new SQLServerConnectionStringBuilder(definition);
		}

		/// <summary>
		///     Validates the Connection Definition.
		/// </summary>
		/// <param name="definition">The definition.</param>
		/// <param name="validationResult">The validation result.</param>
		/// <returns><c>true</c> if required fields are provided., <c>false</c> otherwise.</returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public static bool ValidateDefinition(IDbConnectionDefinition definition, IList<ValidationResult> validationResult)
		{
			var members = new List<string>();

			if (string.IsNullOrWhiteSpace(definition.DataSource))
				members.Add("DataSource");

			if (string.IsNullOrWhiteSpace(definition.Database))
				members.Add("Database");

			if (!definition.IntegratedSecurity)
			{
				if (string.IsNullOrWhiteSpace(definition.UserName))
					members.Add("UserName");

				if (string.IsNullOrWhiteSpace(definition.Password))
					members.Add("Password");
			}

			if (members.Count == 0)
				return true;

			validationResult.Add(new ValidationResult("DbConnectioDefinition is invalid.", members));

			return false;
		}

		private static string AggregateValidationErrorMessage(IEnumerable<ValidationResult> validationResult)
		{
			return validationResult.Aggregate
				(
					new StringBuilder(),
					(stringBuilder, item) =>
					{
						stringBuilder.AppendLine(item.ErrorMessage);
						if (item.MemberNames != null)
						{
							stringBuilder.AppendLine("Missing members:");
							stringBuilder.Append(string.Join(", ", item.MemberNames));
						}
						return stringBuilder;
					},
					result => result.ToString()
				);
		}

		#endregion

		#region fields

		private readonly IDbConnectionDefinition Definition;

		#endregion

		#region Constructors

		/// <summary>
		///     Creates an instance of SQLServerConnectionStringBuilder
		/// </summary>
		/// <param name="parser">Parses nameValue colection to database connection definition</param>
		public SQLServerConnectionStringBuilder(IConnectionDefinitionParser parser)
		{
			if (parser == null)
				throw new ArgumentNullException("parser");

			this.Definition = parser.ConnectionDefinition;
		}

		/// <summary>
		///     Creates an instance of SQLServerConnectionStringBuilder
		/// </summary>
		/// <param name="definition">Databse connection definition</param>
		protected SQLServerConnectionStringBuilder(IDbConnectionDefinition definition)
		{
			this.Definition = definition;
		}

		#endregion

		#region Methods

		/// <summary>
		///     Validate and the get connection string
		/// </summary>
		/// <returns>The connection string</returns>
		public SqlConnectionStringBuilder GetConnectionString()
		{
			if (this.Definition == null)
				throw new InvalidOperationException("No database connection definition found.");

			var validationResults = new List<ValidationResult>();

			var isValid = SQLServerConnectionStringBuilder.ValidateDefinition(this.Definition, validationResults);

			if (!isValid)
			{
				var aggregteMessage = SQLServerConnectionStringBuilder.AggregateValidationErrorMessage(validationResults);

				throw new InvalidOperationException(aggregteMessage);
			}

			var builder = new SqlConnectionStringBuilder
			{
				DataSource = this.Definition.DataSource,
				InitialCatalog = this.Definition.Database,
				IntegratedSecurity = this.Definition.IntegratedSecurity
			};

			if (!builder.IntegratedSecurity)
			{
				builder.UserID = this.Definition.UserName;
				builder.Password = this.Definition.Password;
			}
			builder.MultipleActiveResultSets = true;
			return builder;
		}

		#endregion
	}
}