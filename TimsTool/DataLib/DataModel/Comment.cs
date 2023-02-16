using Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DataLib
{
    [Serializable]
    public sealed class Comment : DataAudit, IEquatable<Comment>
    {
        public Comment(Guid? parentUUID, Guid? parentMasterUUID) 
        { 
            UUID = Guid.NewGuid(); 
            CreatedDate = DateTime.Now;
            LastModifiedDate = DateTime.Now;
            CreatedUser = IdentityHelper.SignedInUser.Email;
            LastModifiedUser = IdentityHelper.SignedInUser.Email;
            ParentUUID = parentUUID;
            ParentMasterUUID = parentMasterUUID;
        }

        [JsonProperty("parentUUID")]
        public Guid? ParentUUID { get; }

        [JsonProperty("parentMasterUUID")]
        public Guid? ParentMasterUUID { get; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("systemCommentType")]
        public string SystemCommentType { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Comment);
        }

        public override int GetHashCode()
        {
            var hashCode = 1338136996;

            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Note);
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid?>.Default.GetHashCode(ParentUUID);
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid?>.Default.GetHashCode(ParentMasterUUID);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SystemCommentType);
            return hashCode;
        }

        public bool Equals(Comment other)
        {
            return other != null &&
                    string.Equals(Note, other.Note, StringComparison.InvariantCulture) &&
                    string.Equals(SystemCommentType, other.SystemCommentType, StringComparison.InvariantCulture) &&
                    ParentUUID == other.ParentUUID &&
                    ParentMasterUUID == other.ParentMasterUUID;
        }

        public static bool operator ==(Comment comment1, Comment comment2)
        {
            return EqualityComparer<Comment>.Default.Equals(comment1, comment2);
        }

        public static bool operator !=(Comment comment1, Comment comment2)
        {
            return !(comment1 == comment2);
        }
    }
}