using InoreaderCs.Entities;
using Unfucked.HTTP.Exceptions;

namespace InoreaderCs.Requests;

internal partial class Requests {

    /// <inheritdoc />
    async Task<User> IInoreaderClient.IUserMethods.GetSelf(CancellationToken cancellationToken) {
        try {
            return await ApiBase
                .Path("user-info")
                .Get<User>(cancellationToken)
                .ConfigureAwait(false);
        } catch (HttpException e) {
            throw TransformError(e, "Failed to get self user info");
        }
    }

}