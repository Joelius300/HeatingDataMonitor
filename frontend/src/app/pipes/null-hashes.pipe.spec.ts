import { NullHashesPipe } from './null-hashes.pipe';

describe('NullHashesPipe', () => {
  it('create an instance', () => {
    const pipe = new NullHashesPipe();
    expect(pipe).toBeTruthy();
  });
});
