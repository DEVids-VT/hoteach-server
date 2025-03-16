using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoTeach.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface: Mongo Entity
    /// </summary>
    public interface IMongoEntity
    {
        /// <summary>
        /// Gets or sets the Unique Identifier
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the Created Date
        /// </summary>
        DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Updated Date
        /// </summary>
        DateTime UpdatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Row Version
        /// </summary>
        int RowVersion { get; set; }

        /// <summary>
        /// Gets or sets the IsDeleted value
        /// </summary>
        bool IsDeleted { get; }

        /// <summary>
        /// Gets or sets the Deleted Date
        /// </summary>
        DateTime? DeletedDateTime { get; set; }
    }
}
