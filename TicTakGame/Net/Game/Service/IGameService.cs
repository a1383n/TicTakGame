using System;
using System.Threading.Tasks;

using TicTakGame.Net.Packet;

namespace TicTakGame.Net.Game
{
    public interface IGameService
    {
        public Task<IPacket> getPacket();
        public Task sendPacket(IPacket packet);
    }
}