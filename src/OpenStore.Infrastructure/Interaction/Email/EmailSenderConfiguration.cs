namespace OpenStore.Infrastructure.Interaction.Email
{
    public abstract class EmailSenderConfiguration
    {
        public virtual string DefaultFromAddress { get; set; }

        public virtual string DefaultFromDisplayName { get; set; }
    }
}